using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Buổi học cụ thể (instance). Tạo recurring bằng cách bulk-insert nhiều Lesson cùng lúc.
    /// ChargeAmount là snapshot tại thời điểm tạo — đổi PerLessonRate trên Student không ảnh hưởng buổi cũ.
    /// </summary>
    public class Lesson : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid IdStudent { get; set; }

        /// <summary>Môn/lớp của buổi này (StudentCourse). Null = buổi cũ trước khi có course.</summary>
        public Guid? IdCourse { get; set; }

        /// <summary>Chỉ phần ngày (component time bỏ qua). Giờ tách ra StartTime/EndTime.</summary>
        public DateTime ScheduledDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        public LessonStatusEnums Status { get; set; } = LessonStatusEnums.Scheduled;

        /// <summary>VND, snapshot từ Student.PerLessonRate khi tạo Lesson.</summary>
        public decimal ChargeAmount { get; set; }

        public DateTime? DoneAt { get; set; }

        public string? Notes { get; set; }

        /// <summary>FK đến TuitionPeriod khi buổi này đã được gom vào kỳ học phí (locked).</summary>
        public Guid? IdTuitionPeriod { get; set; }
    }
}
