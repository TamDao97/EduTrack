using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class TuitionPeriodDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdStudent { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Adjustment { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public TuitionPeriodStatusEnums Status { get; set; } = TuitionPeriodStatusEnums.Open;
        public string? Notes { get; set; }
    }

    public class TuitionPeriodDetailDto : TuitionPeriodDto
    {
        public string? StudentFullName { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhone { get; set; }
    }

    public class TuitionPeriodGridFilter : GridFilterBase
    {
        public Guid? IdStudent { get; set; }
        public TuitionPeriodStatusEnums? Status { get; set; }
        public int? PeriodMonth { get; set; }
        public int? PeriodYear { get; set; }
    }

    /// <summary>
    /// Preview tổng số buổi đã dạy + số tiền dự kiến cho 1 HS trong tháng-năm,
    /// để tutor xem trước khi bấm "Phát hành kỳ".
    /// </summary>
    public class TuitionPreviewDto
    {
        public Guid IdStudent { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalAmount { get; set; }
        /// <summary>Số buổi trong tháng còn "Đã lên lịch" (chưa đánh dấu Đã dạy) — để FE gợi ý vì sao 0 buổi.</summary>
        public int ScheduledLessons { get; set; }
        public List<TuitionPreviewLineDto> Lessons { get; set; } = new();
    }

    public class TuitionPreviewLineDto
    {
        public Guid IdLesson { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
    }
}
