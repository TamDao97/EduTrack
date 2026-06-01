using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    /// <summary>1 dòng trong table "Danh sách gia sư" của founder admin.</summary>
    public class TutorWithSubDto
    {
        public Guid IdTutor { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime? SignupAt { get; set; }

        public Guid? IdSubscription { get; set; }
        public PlanCodeEnums Plan { get; set; }
        public SubscriptionStatusEnums Status { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public int? DaysRemaining { get; set; }

        public int StudentCount { get; set; }
        public int LessonCountThisMonth { get; set; }
        public decimal RevenueLifetime { get; set; }
    }

    public class AdminTutorFilter : GridFilterBase
    {
        public PlanCodeEnums? Plan { get; set; }
        public SubscriptionStatusEnums? Status { get; set; }
    }

    public class ConfirmPaymentReq
    {
        public Guid IdTutor { get; set; }
        public PlanCodeEnums Plan { get; set; } = PlanCodeEnums.Basic;
        public int Months { get; set; } = 1;
        public decimal Amount { get; set; }
        public string? TransferRef { get; set; }
        public string? Notes { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalTutors { get; set; }
        public int TrialCount { get; set; }
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
        public int CancelledCount { get; set; }
        public int SignupsThisMonth { get; set; }
        public decimal MrrEstimate { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLifetime { get; set; }
    }

    public class AdminPaymentRow
    {
        public Guid Id { get; set; }
        public Guid IdTutor { get; set; }
        public string TutorDisplayName { get; set; } = string.Empty;
        public string TutorUserName { get; set; } = string.Empty;
        public PlanCodeEnums Plan { get; set; }
        public int Months { get; set; }
        public decimal Amount { get; set; }
        public string? TransferRef { get; set; }
        public string? Notes { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    /// <summary>Subscription của tutor đang login — cho /billing page.</summary>
    public class MySubscriptionDto
    {
        public Guid Id { get; set; }
        public PlanCodeEnums Plan { get; set; }
        public SubscriptionStatusEnums Status { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? DaysRemaining { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal BasicPrice { get; set; }
        public decimal ProPrice { get; set; }
    }

    public class MyPaymentDto
    {
        public Guid Id { get; set; }
        public PlanCodeEnums Plan { get; set; }
        public int Months { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? Notes { get; set; }
    }
}
