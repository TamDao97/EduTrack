using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class StudentDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdParent { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateBirth { get; set; }
        public string? Grade { get; set; }
        public string? Subject { get; set; }
        public decimal PerLessonRate { get; set; }
        public Guid? AvatarFileId { get; set; }
        public StudentStatusEnums Status { get; set; } = StudentStatusEnums.Active;
        public DateTime? StartedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>DTO mở rộng: kèm thông tin Parent + avatar URL để hiển thị grid/detail.</summary>
    public class StudentDetailDto : StudentDto
    {
        public string? ParentFullName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? AvatarUrl { get; set; }
        /// <summary>Các môn ĐANG HỌC (StudentCourse active), nối " · " — vd "Toán · Lý". Thay cho Subject cũ trên UI.</summary>
        public string? CourseSubjects { get; set; }
    }

    public class StudentGridFilter : GridFilterBase
    {
        public StudentStatusEnums? Status { get; set; }
        public Guid? IdParent { get; set; }
    }
}
