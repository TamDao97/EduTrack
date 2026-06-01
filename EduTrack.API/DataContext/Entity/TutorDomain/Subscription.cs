using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Gói đăng ký SaaS của 1 tutor. Mỗi tutor có 1 Subscription (1:1 với User TUTOR).
    /// Workflow: Signup → tạo Trial 14d → Trial hết → Expired (đọc-only). Admin confirm
    /// thanh toán → Active period mới (cộng tháng vào CurrentPeriodEnd).
    /// </summary>
    public class Subscription : BaseEntity
    {
        public Guid IdTutor { get; set; }

        public PlanCodeEnums Plan { get; set; } = PlanCodeEnums.Basic;

        public SubscriptionStatusEnums Status { get; set; } = SubscriptionStatusEnums.Trial;

        /// <summary>Khi nào Trial kết thúc (chỉ ý nghĩa khi Status=Trial).</summary>
        public DateTime TrialEndsAt { get; set; }

        /// <summary>Khi nào period hiện tại hết hạn (chỉ ý nghĩa khi Status=Active).</summary>
        public DateTime? CurrentPeriodEnd { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
