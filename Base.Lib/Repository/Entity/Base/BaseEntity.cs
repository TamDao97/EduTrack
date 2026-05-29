using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TD.Lib.Repository.Entity.Base
{
    public abstract partial class BaseEntity : ITrackDirty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? CreatedUserId { get; set; }
        public string? CreatedUserName { get; set; }
        public DateTime? DateCreated { get; set; }
        public Guid? ModifyUserId { get; set; }
        public string? ModifyUserName { get; set; }
        public DateTime? DateModify { get; set; }
        public Guid? DeletedUserId { get; set; }
        public string? DeletedUserName { get; set; }
        public DateTime? DateDeleted { get; set; }
        public bool IsDeleted { get; set; }
        public int? Order { get; set; }
    }
}