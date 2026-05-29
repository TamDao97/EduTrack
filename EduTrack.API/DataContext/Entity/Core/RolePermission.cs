using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.Core
{
    public class RolePermission : BaseEntity
    {
        public Guid IdRole { get; set; }
        public Guid IdPermission { get; set; }
    }
}
