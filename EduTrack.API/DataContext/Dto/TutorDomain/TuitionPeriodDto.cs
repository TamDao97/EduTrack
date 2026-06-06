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
        /// <summary>Số buổi TỒN từ các tháng trước được gộp vào hoá đơn này.</summary>
        public int CarryoverLessons { get; set; }
        public List<TuitionPreviewLineDto> Lessons { get; set; } = new();
    }

    /// <summary>
    /// Bảng chốt kỳ THÁNG: hệ thống tự quét HS có buổi Đã dạy chưa chốt,
    /// tutor chỉ duyệt + bấm 1 nút — thay vì đi tìm từng em.
    /// </summary>
    public class MonthClosePreviewDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalAmount { get; set; }
        /// <summary>Số buổi ĐÃ QUA còn "Đã lên lịch" (quên đánh dấu) — cảnh báo trước khi chốt.</summary>
        public int PastScheduledLessons { get; set; }
        public List<MonthCloseCandidateDto> Candidates { get; set; } = new();
    }

    /// <summary>1 HS đủ điều kiện tính học phí trong tháng.</summary>
    public class MonthCloseCandidateDto
    {
        public Guid IdStudent { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        /// <summary>Tổng buổi sẽ vào hoá đơn (gồm cả buổi tồn tháng trước).</summary>
        public int DoneLessons { get; set; }
        /// <summary>Số buổi TỒN từ các tháng trước (phát sinh sau khi kỳ cũ đã tính).</summary>
        public int CarryoverLessons { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthCloseBulkReq
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<Guid> StudentIds { get; set; } = new();
    }

    public class MonthCloseResultDto
    {
        public int ClosedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>1 đợt thu trong lịch sử thanh toán của kỳ.</summary>
    public class TuitionPaymentDto : Base.BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdPeriod { get; set; }
        public decimal Amount { get; set; }
        public string? Method { get; set; }
        public string? Notes { get; set; }
    }

    public class TuitionPreviewLineDto
    {
        public Guid IdLesson { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
        /// <summary>Môn của buổi — để hoá đơn breakdown theo môn (null = buổi cũ).</summary>
        public string? Subject { get; set; }
    }
}
