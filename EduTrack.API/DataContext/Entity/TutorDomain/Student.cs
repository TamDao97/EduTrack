using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>Học sinh — đối tượng dạy của gia sư.</summary>
    public class Student : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid IdParent { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateBirth { get; set; }

        /// <summary>Lớp/trình độ: "Lớp 9", "Lớp 12", "Đại học"…</summary>
        [MaxLength(50)]
        public string? Grade { get; set; }

        /// <summary>Môn đang dạy HS này: "Toán", "Lý"…</summary>
        [MaxLength(100)]
        public string? Subject { get; set; }

        /// <summary>Học phí mỗi buổi (VND). Mặc định khi tạo Lesson sẽ copy snapshot.</summary>
        public decimal PerLessonRate { get; set; }

        public Guid? AvatarFileId { get; set; }

        public StudentStatusEnums Status { get; set; } = StudentStatusEnums.Active;

        public DateTime? StartedAt { get; set; }

        public string? Notes { get; set; }
    }
}
