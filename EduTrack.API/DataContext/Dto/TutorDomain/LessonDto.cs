using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class LessonDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdStudent { get; set; }
        public Guid? IdCourse { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public LessonStatusEnums Status { get; set; } = LessonStatusEnums.Scheduled;
        public decimal ChargeAmount { get; set; }
        public DateTime? DoneAt { get; set; }
        public string? Notes { get; set; }
        public Guid? IdTuitionPeriod { get; set; }
        public Guid? GroupKey { get; set; }
        public Guid? IdClass { get; set; }
    }

    /// <summary>Lesson kèm tên HS — dùng cho lịch tuần / inbox nhắc.</summary>
    public class LessonDetailDto : LessonDto
    {
        public string? StudentFullName { get; set; }
        public string? ParentPhone { get; set; }
        /// <summary>Tên môn của buổi (từ StudentCourse hoặc môn của Lớp) — null nếu chưa gắn.</summary>
        public string? CourseSubject { get; set; }
        /// <summary>Tên lớp nếu buổi sinh từ ClassRoom — hiện badge trên lịch.</summary>
        public string? ClassName { get; set; }
    }

    public class LessonGridFilter : GridFilterBase
    {
        public Guid? IdStudent { get; set; }
        public LessonStatusEnums? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>1 HS trong buổi nhóm — môn riêng (giá lấy theo môn của từng em).</summary>
    public class GroupStudentReq
    {
        public Guid IdStudent { get; set; }
        public Guid? IdCourse { get; set; }
    }

    /// <summary>
    /// Request tạo buổi NHÓM: nhiều HS học chung 1 ca. Mỗi HS sinh 1 Lesson riêng
    /// (cùng GroupKey) — giá theo môn của từng em. NumberOfWeeks > 1 = lặp hàng tuần.
    /// </summary>
    public class LessonGroupCreateReq
    {
        public List<GroupStudentReq> Students { get; set; } = new();
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public int NumberOfWeeks { get; set; } = 1;
    }

    /// <summary>Request tạo nhiều Lesson recurring 1 lần (vd 12 tuần liên tiếp).</summary>
    public class LessonBulkCreateReq
    {
        public Guid IdStudent { get; set; }
        public Guid? IdCourse { get; set; }
        public DateTime StartDate { get; set; }
        public int NumberOfWeeks { get; set; } = 12;
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
    }
}
