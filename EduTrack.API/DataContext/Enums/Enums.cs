using System.ComponentModel;

namespace EduTrack.API.DataContext.Enums
{
    public enum GenderEnums
    {
        [Description("Nam")]
        Male,
        [Description("Nữ")]
        FeMale,
        [Description("Khác")]
        Orther
    }

    public enum StudentStatusEnums
    {
        [Description("Đang học")]
        Active = 1,
        [Description("Tạm nghỉ")]
        Paused = 2,
        [Description("Đã dừng")]
        Stopped = 3,
    }

    public enum LessonStatusEnums
    {
        [Description("Sắp tới")]
        Scheduled = 1,
        [Description("Đã dạy")]
        Done = 2,
        [Description("Đã huỷ")]
        Cancelled = 3,
    }

    public enum TuitionPeriodStatusEnums
    {
        [Description("Đang mở (chưa chốt)")]
        Open = 1,
        [Description("Đã chốt (chờ thu)")]
        Closed = 2,
        [Description("Thu một phần")]
        PartialPaid = 3,
        [Description("Đã thu đủ")]
        Paid = 4,
    }

    public enum NotificationTypeEnums
    {
        [Description("Nhắc lịch học (tối hôm trước)")]
        LessonReminderEvening = 1,
        [Description("Nhắc lịch học (trước 1 giờ)")]
        LessonReminderHourBefore = 2,
        [Description("Thông báo học phí")]
        TuitionIssued = 3,
        [Description("Nhắc nợ học phí")]
        TuitionOverdue = 4,
    }

    public enum NotificationStatusEnums
    {
        [Description("Chờ gửi")]
        Pending = 1,
        [Description("Đã gửi")]
        Sent = 2,
        [Description("Đã đọc")]
        Read = 3,
        [Description("Đã huỷ")]
        Cancelled = 4,
        /// <summary>Quá hạn 72h chưa được gửi → marked Missed bởi background dispatcher.</summary>
        [Description("Đã bỏ lỡ")]
        Missed = 5,
    }

    public enum PlanCodeEnums
    {
        [Description("Miễn phí")]
        Free = 1,
        [Description("Cơ bản")]
        Basic = 2,
        [Description("Chuyên nghiệp")]
        Pro = 3,
    }

    public enum SubscriptionStatusEnums
    {
        [Description("Dùng thử")]
        Trial = 1,
        [Description("Đang hoạt động")]
        Active = 2,
        [Description("Hết hạn")]
        Expired = 3,
        [Description("Đã huỷ")]
        Cancelled = 4,
    }

    public enum PaymentStatusEnums
    {
        [Description("Chờ xác nhận")]
        Pending = 1,
        [Description("Đã xác nhận")]
        Confirmed = 2,
        [Description("Hoàn tiền")]
        Refunded = 3,
    }
}
