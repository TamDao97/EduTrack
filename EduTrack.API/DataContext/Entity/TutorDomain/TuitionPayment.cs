using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// LỊCH SỬ THU TIỀN học phí: mỗi lần tutor ghi nhận thanh toán = 1 dòng
    /// (phụ huynh trả 3 đợt → 3 dòng). TuitionPeriod.PaidAmount = tổng các dòng.
    /// Ngày thu = DateCreated (giờ VN).
    /// </summary>
    public class TuitionPayment : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        /// <summary>Kỳ học phí (TuitionPeriod) được thu.</summary>
        public Guid IdPeriod { get; set; }

        /// <summary>Số tiền THỰC GHI NHẬN đợt này (đã clamp nếu nhập dư).</summary>
        public decimal Amount { get; set; }

        /// <summary>Hình thức: "Chuyển khoản" / "Tiền mặt" / "Khác".</summary>
        [MaxLength(50)]
        public string? Method { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Nếu là dòng HOÀN TÁC: trỏ về đợt thu gốc bị hoàn (Amount mang dấu âm).
        /// Null = đợt thu thường. Không xoá dòng gốc — sổ sách giữ đủ cả thu lẫn hoàn.
        /// </summary>
        public Guid? IdReversalOf { get; set; }
    }
}
