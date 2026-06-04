using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Buổi học cụ thể (instance). Tạo recurring bằng cách bulk-insert nhiều Lesson cùng lúc.
    /// ChargeAmount là snapshot tại thời điểm tạo (giá môn/lớp) — đổi giá sau không ảnh hưởng buổi cũ.
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

        /// <summary>VND, snapshot từ giá môn (StudentCourse) hoặc giá lớp khi tạo Lesson.</summary>
        public decimal ChargeAmount { get; set; }

        public DateTime? DoneAt { get; set; }

        public string? Notes { get; set; }

        /// <summary>FK đến TuitionPeriod khi buổi này đã được gom vào kỳ học phí (locked).</summary>
        public Guid? IdTuitionPeriod { get; set; }

        /// <summary>
        /// Buổi NHÓM: các Lesson cùng 1 ca dạy (nhiều HS học chung) chia sẻ cùng GroupKey.
        /// Mỗi HS vẫn có dòng Lesson riêng (giá riêng, nhắc riêng, học phí riêng) — GroupKey
        /// chỉ để gắn kết hiển thị + tạo 1 lần. Null = buổi 1-1 bình thường.
        /// </summary>
        public Guid? GroupKey { get; set; }

        /// <summary>Buổi sinh từ LỚP HỌC nào (ClassRoom) — null = buổi 1-1 / nhóm ad-hoc.</summary>
        public Guid? IdClass { get; set; }
    }
}
