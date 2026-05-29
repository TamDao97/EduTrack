using EduTrack.API.DataContext.Dto.Base;

namespace EduTrack.API.DataContext.Dto.Core
{
    public class PermissionDto : BaseDto
    {
        public string ModuleCode { get; set; }
        public string ModuleDescription { get; set; }
        public int ModuleOrder { get; set; }

        public string PermissionCode { get; set; }
        public string? Description { get; set; }
        public string? RoleCodes { get; set; }
    }

    public class PermissionViewDto : PermissionDto
    {
        public bool IsChecked { get; set; }
    }
}
