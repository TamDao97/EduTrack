namespace EduTrack.API.DataContext.Dto.Base
{
    public class BaseDto
    {
        public Guid? Id { get; set; }
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
