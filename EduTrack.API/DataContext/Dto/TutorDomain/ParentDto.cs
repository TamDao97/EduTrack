using EduTrack.API.DataContext.Dto.Base;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class ParentDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Notes { get; set; }
    }

    public class ParentGridFilter : GridFilterBase
    {
    }
}
