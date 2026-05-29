using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.Core
{
    public class UserRole : BaseEntity
    {
        /// <summary>
        /// </summary>
        public Guid IdUser { get; set; }

        /// <summary>
        /// </summary>
        public Guid IdRole { get; set; }
    }
}
