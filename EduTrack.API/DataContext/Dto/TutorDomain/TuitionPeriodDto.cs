using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class TuitionPeriodDto : BaseDto
    {
        public Guid IdTutor { get; set; }
        public Guid IdStudent { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Adjustment { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public TuitionPeriodStatusEnums Status { get; set; } = TuitionPeriodStatusEnums.Open;
        public string? Notes { get; set; }
    }

    public class TuitionPeriodDetailDto : TuitionPeriodDto
    {
        public string? StudentFullName { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhone { get; set; }
    }

    public class TuitionPeriodGridFilter : GridFilterBase
    {
        public Guid? IdStudent { get; set; }
        public TuitionPeriodStatusEnums? Status { get; set; }
        public int? PeriodMonth { get; set; }
        public int? PeriodYear { get; set; }
    }
}
