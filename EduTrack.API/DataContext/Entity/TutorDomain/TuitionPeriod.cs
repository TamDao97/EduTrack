using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Kỳ học phí — gom các Lesson đã dạy trong tháng của 1 HS thành 1 hoá đơn để thu tiền.
    /// Khi `Status = Closed` thì các Lesson trong kỳ bị lock (không sửa được nữa).
    /// </summary>
    public class TuitionPeriod : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid IdStudent { get; set; }

        public int PeriodMonth { get; set; }

        public int PeriodYear { get; set; }

        /// <summary>Khi tutor bấm "Phát hành" → set ClosedAt và lock các Lesson trong kỳ.</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>Số buổi đã dạy trong kỳ (snapshot tại lúc close).</summary>
        public int TotalLessons { get; set; }

        /// <summary>Sum(Lesson.ChargeAmount) của các buổi Done trong kỳ.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Cộng (+) thưởng / trừ (−) giảm giá. Tutor chỉnh tay.</summary>
        public decimal Adjustment { get; set; }

        public decimal FinalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        /// <summary>Computed — không lưu DB.</summary>
        [NotMapped]
        public decimal OutstandingAmount => FinalAmount - PaidAmount;

        public TuitionPeriodStatusEnums Status { get; set; } = TuitionPeriodStatusEnums.Open;

        public string? Notes { get; set; }
    }
}
