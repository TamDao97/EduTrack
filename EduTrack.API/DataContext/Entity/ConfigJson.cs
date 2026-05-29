using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity
{
    public class ConfigJson : BaseEntity
    {
        public string? JsonOrg { get; set; }
        public string? JsonSupperAdmin { get; set; }
    }
}