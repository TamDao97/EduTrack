using EduTrack.API.DataContext.Dto.Base;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    /// <summary>Filter danh sách lớp: keyword (tên/môn) + trạng thái + môn + khoảng khai giảng + paging.</summary>
    public class ClassRoomGridFilter : GridFilterBase
    {
        public bool? IsActive { get; set; }
        /// <summary>Lọc đúng môn (so khớp không phân biệt hoa thường).</summary>
        public string? Subject { get; set; }
        /// <summary>Khai giảng TỪ ngày (lớp chưa khai ngày bị loại khi có lọc thời gian).</summary>
        public DateTime? StartFrom { get; set; }
        /// <summary>Khai giảng ĐẾN ngày.</summary>
        public DateTime? StartTo { get; set; }
    }

    public class ClassRoomDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public decimal DefaultRatePerLesson { get; set; }
        /// <summary>CSV .NET DayOfWeek (0=CN..6=T7), vd "1,3,5" = T2-T4-T6.</summary>
        public string DaysOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        /// <summary>Số HS đang ghi danh — fill khi list.</summary>
        public int MemberCount { get; set; }
    }

    /// <summary>Chi tiết lớp: kèm danh sách ghi danh.</summary>
    public class ClassRoomDetailDto : ClassRoomDto
    {
        public List<ClassMemberDto> Members { get; set; } = new();
    }

    public class ClassMemberDto : BaseDto
    {
        public Guid IdClass { get; set; }
        public Guid IdStudent { get; set; }
        public string? StudentFullName { get; set; }
        public decimal? RateOverride { get; set; }
        /// <summary>Giá thực tế = RateOverride ?? giá lớp.</summary>
        public decimal EffectiveRate { get; set; }
    }

    public class ClassMemberReq
    {
        public Guid IdClass { get; set; }
        public Guid IdStudent { get; set; }
        public decimal? RateOverride { get; set; }
    }

    public class GenerateScheduleReq
    {
        public Guid IdClass { get; set; }
        public DateTime StartDate { get; set; }
        public int NumberOfWeeks { get; set; } = 4;
    }

    /// <summary>Lớp mà 1 HS đang ghi danh — hiện ở Chi tiết HS.</summary>
    public class StudentClassDto
    {
        public Guid IdClass { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public decimal EffectiveRate { get; set; }
        public string ScheduleLabel { get; set; } = string.Empty;
    }
}
