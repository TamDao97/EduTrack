using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// LỚP HỌC — khái niệm trung tâm cho tutor dạy nhóm: "Lớp Toán 1A, T2-4-6 ca 7-9h,
    /// 200k/buổi/HS". Có danh sách ghi danh (ClassMember) + lịch tuần cố định;
    /// bấm "Xếp lịch N tuần" sinh Lesson cho từng em (giá riêng em ?? giá lớp).
    /// Song song với StudentCourse (kèm 1-1) — không thay thế.
    /// </summary>
    public class ClassRoom : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        /// <summary>Tên lớp: "Toán 1A", "Anh văn 5B"…</summary>
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Môn của lớp — dùng cho chip môn + báo cáo doanh thu theo môn.</summary>
        [MaxLength(100)]
        public string? Subject { get; set; }

        /// <summary>Giá mặc định mỗi buổi MỖI HS — em đặc biệt override ở ClassMember.</summary>
        public decimal DefaultRatePerLesson { get; set; }

        /// <summary>Các thứ trong tuần, csv theo .NET DayOfWeek (0=CN..6=T7). Vd "1,3,5" = T2-T4-T6.</summary>
        [MaxLength(20)]
        public string DaysOfWeek { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        /// <summary>Ngày khai giảng (mở lớp) — null = không xác định.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Ngày kết thúc khoá — null = dạy dài hạn. Xếp lịch không sinh buổi sau ngày này.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>false = lớp đã đóng (không xếp lịch mới).</summary>
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
