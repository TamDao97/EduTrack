using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Lịch sử thanh toán SaaS — mỗi lần tutor chuyển khoản gia hạn, admin tạo 1 record này.
    /// </summary>
    public class SubscriptionPayment : BaseEntity
    {
        public Guid IdTutor { get; set; }
        public Guid IdSubscription { get; set; }

        public PlanCodeEnums Plan { get; set; }

        /// <summary>Số tháng được mua/gia hạn — 1, 3, 6, 12.</summary>
        public int Months { get; set; }

        /// <summary>Số tiền VND.</summary>
        public decimal Amount { get; set; }

        public PaymentStatusEnums Status { get; set; } = PaymentStatusEnums.Confirmed;

        /// <summary>Mã giao dịch / ghi chú số tiền tutor chuyển — admin ghi từ app bank.</summary>
        [MaxLength(200)]
        public string? TransferRef { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public Guid? ConfirmedByAdmin { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
