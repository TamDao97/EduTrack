using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.Core
{
    public class Permission : BaseEntity
    {
        public string ModuleCode { get; set; }
        public string ModuleDescription { get; set; }
        public int ModuleOrder { get; set; }

        public string PermissionCode { get; set; }
        public string? Description { get; set; }
        public string? RoleCodes { get; set; }
    }
}
