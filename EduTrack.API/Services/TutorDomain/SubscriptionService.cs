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
    }
}
