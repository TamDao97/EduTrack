using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    /// <summary>
    /// Quản lý vòng đời Subscription. Gọi từ AuthService (EnsureTrial khi signup) +
    /// AdminService (extend khi confirm payment) + tutor self-query (/billing).
    /// </summary>
    public interface ISubscriptionService
    {
        Task<Subscription> EnsureTrialAsync(Guid idTutor, int trialDays = 14);
        Task<Subscription?> GetByTutorAsync(Guid idTutor);
        Task<MySubscriptionDto?> GetMineAsync(Guid idTutor);
        Task<List<MyPaymentDto>> GetMyPaymentsAsync(Guid idTutor);
        Task ExtendAsync(Guid idSubscription, PlanCodeEnums plan, int months);
        decimal PriceOf(PlanCodeEnums plan);

        /// <summary>Giới hạn HS theo gói. Free=5, Basic=20, Pro=không giới hạn.</summary>
        int MaxStudentsOf(PlanCodeEnums plan);

        /// <summary>
        /// Kiểm tra tutor có còn quyền ghi không (Trial chưa hết, Active chưa hết hạn).
        /// Trả null nếu OK; trả message nếu bị chặn — caller dùng làm Response.Error.
        /// </summary>
        Task<string?> CheckCanWriteAsync(Guid idTutor);

        /// <summary>
        /// Kiểm tra có thể thêm 1 HS nữa không. Combines CheckCanWrite + quota check.
        /// </summary>
        Task<string?> CheckCanAddStudentAsync(Guid idTutor, int currentStudentCount);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITDRepository<Subscription> _repos;
        private readonly ITDRepository<SubscriptionPayment> _paymentRepos;

        public SubscriptionService(IUnitOfWork uow)
        {
            _uow = uow;
            _repos = uow.GetRepository<Subscription>();
            _paymentRepos = uow.GetRepository<SubscriptionPayment>();
        }

        /// <summary>Tạo Trial 14 ngày nếu tutor chưa có sub. Idempotent.</summary>
        public async Task<Subscription> EnsureTrialAsync(Guid idTutor, int trialDays = 14)
        {
            var existed = await _repos.Table.FirstOrDefaultAsync(s => s.IdTutor == idTutor);
            if (existed != null) return existed;

            var sub = new Subscription
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                Plan = PlanCodeEnums.Basic,
                Status = SubscriptionStatusEnums.Trial,
                TrialEndsAt = DateTime.UtcNow.AddDays(trialDays),
            };
            await _repos.CreateAsync(sub);
            await _uow.SaveChangesAsync();
            return sub;
        }

        public async Task<Subscription?> GetByTutorAsync(Guid idTutor)
            => await _repos.TableNoTracking.FirstOrDefaultAsync(s => s.IdTutor == idTutor);

        /// <summary>Tutor xem subscription của chính mình — auto-create Trial nếu chưa có.</summary>
        public async Task<MySubscriptionDto?> GetMineAsync(Guid idTutor)
        {
            var sub = await EnsureTrialAsync(idTutor);
            var now = DateTime.UtcNow;

            var effectiveStatus = sub.Status;
            DateTime? expiresAt = null;
            switch (sub.Status)
            {
                case SubscriptionStatusEnums.Trial:
                    expiresAt = sub.TrialEndsAt;
                    if (sub.TrialEndsAt < now) effectiveStatus = SubscriptionStatusEnums.Expired;
                    break;
                case SubscriptionStatusEnums.Active:
                    expiresAt = sub.CurrentPeriodEnd;
                    if (sub.CurrentPeriodEnd == null || sub.CurrentPeriodEnd < now)
                        effectiveStatus = SubscriptionStatusEnums.Expired;
                    break;
            }

            int? daysRemaining = expiresAt.HasValue
                ? (int)Math.Ceiling((expiresAt.Value - now).TotalDays)
                : null;

            return new MySubscriptionDto
            {
                Id = sub.Id,
                Plan = sub.Plan,
                Status = effectiveStatus,
                TrialEndsAt = sub.TrialEndsAt,
                CurrentPeriodEnd = sub.CurrentPeriodEnd,
                ExpiresAt = expiresAt,
                DaysRemaining = daysRemaining,
                CurrentPrice = PriceOf(sub.Plan),
                BasicPrice = PriceOf(PlanCodeEnums.Basic),
                ProPrice = PriceOf(PlanCodeEnums.Pro),
            };
        }

        public async Task<List<MyPaymentDto>> GetMyPaymentsAsync(Guid idTutor)
        {
            return await _paymentRepos.TableNoTracking
                .Where(p => p.IdTutor == idTutor && p.Status == PaymentStatusEnums.Confirmed)
                .OrderByDescending(p => p.ConfirmedAt ?? p.DateCreated)
                .Select(p => new MyPaymentDto
                {
                    Id = p.Id,
                    Plan = p.Plan,
                    Months = p.Months,
                    Amount = p.Amount,
                    ConfirmedAt = p.ConfirmedAt,
                    Notes = p.Notes,
                }).ToListAsync();
        }

        public async Task ExtendAsync(Guid idSubscription, PlanCodeEnums plan, int months)
        {
            var sub = await _repos.Table.FirstOrDefaultAsync(s => s.Id == idSubscription);
            if (sub == null) throw new InvalidOperationException("Subscription không tồn tại");

            var now = DateTime.UtcNow;
            var startFrom = (sub.CurrentPeriodEnd.HasValue && sub.CurrentPeriodEnd.Value > now)
                ? sub.CurrentPeriodEnd.Value
                : now;

            sub.Plan = plan;
            sub.Status = SubscriptionStatusEnums.Active;
            sub.CurrentPeriodEnd = startFrom.AddMonths(months);
            sub.CancelledAt = null;

            sub.MarkDirty(nameof(sub.Plan));
            sub.MarkDirty(nameof(sub.Status));
            sub.MarkDirty(nameof(sub.CurrentPeriodEnd));
            sub.MarkDirty(nameof(sub.CancelledAt));

            await _uow.SaveChangesAsync();
        }

        public decimal PriceOf(PlanCodeEnums plan) => plan switch
        {
            PlanCodeEnums.Free => 0,
            PlanCodeEnums.Basic => 99_000m,
            PlanCodeEnums.Pro => 199_000m,
            _ => 0,
        };

        public int MaxStudentsOf(PlanCodeEnums plan) => plan switch
        {
            PlanCodeEnums.Free => 5,
            PlanCodeEnums.Basic => 20,
            PlanCodeEnums.Pro => int.MaxValue,
            _ => 5,
        };

        /// <summary>
        /// Trả null = OK. Trả message = đang bị chặn.
        /// Auto-ensure Trial — tutor mới luôn có 14 ngày dùng thử trước khi bị block.
        /// </summary>
        public async Task<string?> CheckCanWriteAsync(Guid idTutor)
        {
            var sub = await EnsureTrialAsync(idTutor);
            var now = DateTime.UtcNow;

            switch (sub.Status)
            {
                case SubscriptionStatusEnums.Trial:
                    if (sub.TrialEndsAt < now)
                        return "Thời gian dùng thử 14 ngày đã hết. Vui lòng nâng cấp gói để tiếp tục.";
                    return null;

                case SubscriptionStatusEnums.Active:
                    if (sub.CurrentPeriodEnd.HasValue && sub.CurrentPeriodEnd.Value < now)
                        return "Subscription đã hết hạn. Vui lòng gia hạn để tiếp tục.";
                    return null;

                case SubscriptionStatusEnums.Expired:
                case SubscriptionStatusEnums.Cancelled:
                    return "Subscription đã hết hạn. Vui lòng nâng cấp gói để tiếp tục.";

                default:
                    return null;
            }
        }

        public async Task<string?> CheckCanAddStudentAsync(Guid idTutor, int currentStudentCount)
        {
            // 1) Phải còn hạn dùng đã
            var writeErr = await CheckCanWriteAsync(idTutor);
            if (writeErr != null) return writeErr;

            // 2) Check quota theo plan
            var sub = await EnsureTrialAsync(idTutor);
            // Trial dùng quota của Basic (=20) — đó là mục đích của trial cho user dùng thử full feature
            var effectivePlan = sub.Status == SubscriptionStatusEnums.Trial
                ? PlanCodeEnums.Basic
                : sub.Plan;
            var max = MaxStudentsOf(effectivePlan);
            if (currentStudentCount >= max)
            {
                return max == int.MaxValue
                    ? "Không thể thêm học sinh."
                    : $"Gói hiện tại chỉ cho phép tối đa {max} học sinh. Bạn đã có {currentStudentCount}. Nâng cấp gói để thêm.";
            }
            return null;
        }
    }
}
