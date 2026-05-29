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
}
