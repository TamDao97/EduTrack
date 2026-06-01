using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    /// <summary>
    /// Quản lý vòng đời Subscription. Gọi từ AuthService (EnsureTrial khi signup) +
    /// AdminService (extend khi confirm payment).
    /// </summary>
    public interface ISubscriptionService
    {
        Task<Subscription> EnsureTrialAsync(Guid idTutor, int trialDays = 14);
        Task<Subscription?> GetByTutorAsync(Guid idTutor);
        Task ExtendAsync(Guid idSubscription, PlanCodeEnums plan, int months);
        decimal PriceOf(PlanCodeEnums plan);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITDRepository<Subscription> _repos;

        public SubscriptionService(IUnitOfWork uow)
        {
            _uow = uow;
            _repos = uow.GetRepository<Subscription>();
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

        /// <summary>
        /// Gia hạn: đổi Plan + cộng N tháng vào CurrentPeriodEnd (hoặc từ now nếu chưa active).
        /// </summary>
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
