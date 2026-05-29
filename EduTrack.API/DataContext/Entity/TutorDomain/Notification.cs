using EduTrack.API.DataContext.Enums;
using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Item nhắc — tutor xem trong inbox và 1-chạm gửi Zalo cho phụ huynh.
    /// Được sinh REACTIVE khi tạo Lesson hoặc Close TuitionPeriod.
    /// Email channel có thể tự động hoá sau bằng cron (W4+).
    /// </summary>
    public class Notification : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid? IdStudent { get; set; }

        public NotificationTypeEnums Type { get; set; }

        /// <summary>FK tới Lesson hoặc TuitionPeriod tuỳ Type.</summary>
        public Guid? RefId { get; set; }

        /// <summary>Tiêu đề ngắn để hiển thị inbox: "Mai · Toán · mai 19h30".</summary>
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Nội dung text đầy đủ để gửi qua Zalo/Email.</summary>
        [Required]
        public string BodyText { get; set; } = string.Empty;

        /// <summary>Deeplink Zalo đã encode sẵn — tutor click 1 phát mở Zalo với text điền sẵn.</summary>
        public string? ZaloDeepLink { get; set; }

        /// <summary>SĐT phụ huynh (snapshot) — phòng trường hợp parent đổi SĐT sau.</summary>
        [MaxLength(20)]
        public string? ParentPhone { get; set; }

        /// <summary>Tên HS (snapshot) — để hiển thị nhanh inbox không cần join.</summary>
        [MaxLength(200)]
        public string? StudentFullName { get; set; }

        public NotificationStatusEnums Status { get; set; } = NotificationStatusEnums.Pending;

        /// <summary>Khi nào nên gửi (vd: T-18h trước buổi học).</summary>
        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }
    }
}
