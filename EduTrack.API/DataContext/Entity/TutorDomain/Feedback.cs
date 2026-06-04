using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Góp ý / báo lỗi / đề xuất tính năng từ tutor.
    /// Founder xem toàn bộ trong /admin, đổi trạng thái + ghi chú phản hồi;
    /// tutor thấy trạng thái + phản hồi trên góp ý của mình.
    /// </summary>
    public class Feedback : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public FeedbackTypeEnums Type { get; set; } = FeedbackTypeEnums.GopY;

        /// <summary>Mức hài lòng 1-5 sao — tuỳ chọn, không bắt buộc.</summary>
        public int? Rating { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public FeedbackStatusEnums Status { get; set; } = FeedbackStatusEnums.Moi;

        /// <summary>Phản hồi của founder — tutor nhìn thấy.</summary>
        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
