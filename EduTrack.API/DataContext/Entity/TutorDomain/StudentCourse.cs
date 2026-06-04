using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Lớp/Môn học của 1 học sinh — cho phép 1 HS học nhiều môn với GIÁ KHÁC NHAU.
    /// Lesson gắn IdCourse để biết buổi đó môn gì + lấy đúng giá; hoá đơn breakdown theo môn.
    /// HS cũ được seed 1 course mặc định từ Student.Subject + PerLessonRate (DataSeeder).
    /// </summary>
    public class StudentCourse : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid IdStudent { get; set; }

        /// <summary>Tên môn/lớp: "Toán", "Lý", "Luyện thi vào 10"…</summary>
        [Required, MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>Giá mỗi buổi của RIÊNG môn này.</summary>
        public decimal PerLessonRate { get; set; }

        /// <summary>false = tạm ngừng môn này (không hiện khi tạo buổi mới).</summary>
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
