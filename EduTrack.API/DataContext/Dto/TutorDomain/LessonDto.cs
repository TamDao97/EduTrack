using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class LessonDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdStudent { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public LessonStatusEnums Status { get; set; } = LessonStatusEnums.Scheduled;
        public decimal ChargeAmount { get; set; }
        public DateTime? DoneAt { get; set; }
        public string? Notes { get; set; }
        public Guid? IdTuitionPeriod { get; set; }
    }

    /// <summary>Lesson kèm tên HS — dùng cho lịch tuần / inbox nhắc.</summary>
    public class LessonDetailDto : LessonDto
    {
        public string? StudentFullName { get; set; }
        public string? ParentPhone { get; set; }
    }

    public class LessonGridFilter : GridFilterBase
    {
        public Guid? IdStudent { get; set; }
        public LessonStatusEnums? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>Request tạo nhiều Lesson recurring 1 lần (vd 12 tuần liên tiếp).</summary>
    public class LessonBulkCreateReq
    {
        public Guid IdStudent { get; set; }
        public DateTime StartDate { get; set; }
        public int NumberOfWeeks { get; set; } = 12;
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
    }
}
