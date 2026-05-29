using EduTrack.API.DataContext.Dto.Base;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.Core
{
    public class RoleDto : BaseDto
    {
        public string Code { get; set; }
        public virtual string Name { get; set; }
        public string? Description { get; set; }
    }

    public class RoleGridFilter : GridFilterBase
    {
    }

    public class PermissionGroupByModuleDto
    {
        public string ModuleCode { get; set; }
        public string ModuleDescription { get; set; }
        public int ModuleOrder { get; set; }
        public List<PermissionViewDto> LstPermissions { get; set; } = new List<PermissionViewDto>();
    }

    public class PermissionGroupByRoleCreateReqDto
    {
        public Guid IdRole { get; set; }
        public List<string> LstPermissions { get; set; } = new List<string>();
    }
}
