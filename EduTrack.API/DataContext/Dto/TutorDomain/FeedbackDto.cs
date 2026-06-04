using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class FeedbackDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public FeedbackTypeEnums Type { get; set; }
        public int? Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public FeedbackStatusEnums Status { get; set; }
        public string? AdminNote { get; set; }
    }

    /// <summary>Dòng góp ý cho bảng admin — kèm tên tutor gửi.</summary>
    public class FeedbackDetailDto : FeedbackDto
    {
        public string? TutorDisplayName { get; set; }
        public string? TutorUserName { get; set; }
    }

    public class FeedbackGridFilter : GridFilterBase
    {
        public FeedbackTypeEnums? Type { get; set; }
        public FeedbackStatusEnums? Status { get; set; }
    }

    public class FeedbackUpdateStatusReq
    {
        public FeedbackStatusEnums Status { get; set; }
        public string? AdminNote { get; set; }
    }
}
