using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class NotificationDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid? IdStudent { get; set; }
        public NotificationTypeEnums Type { get; set; }
        public Guid? RefId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string BodyText { get; set; } = string.Empty;
        public string? ZaloDeepLink { get; set; }
        public string? ParentPhone { get; set; }
        public string? StudentFullName { get; set; }
        public NotificationStatusEnums Status { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

    public class NotificationGridFilter : GridFilterBase
    {
        public NotificationStatusEnums? Status { get; set; }
        public NotificationTypeEnums? Type { get; set; }
        public Guid? IdStudent { get; set; }
    }
}
