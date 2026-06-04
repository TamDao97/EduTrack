using EduTrack.API.DataContext.Dto.Base;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class StudentCourseDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdStudent { get; set; }
        public string Subject { get; set; } = string.Empty;
        public decimal PerLessonRate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
