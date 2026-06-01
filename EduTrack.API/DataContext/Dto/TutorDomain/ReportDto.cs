namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    /// <summary>1 điểm trong line/bar chart theo tháng.</summary>
    public class MonthlyPointDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int LessonsDone { get; set; }
        /// <summary>Tổng FinalAmount của TuitionPeriod ở tháng này (đã chốt).</summary>
        public decimal Revenue { get; set; }
        /// <summary>Tổng PaidAmount đã thu trong tháng này.</summary>
        public decimal Collected { get; set; }
    }

    /// <summary>HS có nhiều buổi nhất / doanh thu cao nhất.</summary>
    public class TopStudentDto
    {
        public Guid IdStudent { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int LessonsDone { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportSummaryDto
    {
        public int TotalStudents { get; set; }
        public int LessonsDoneLifetime { get; set; }
        public decimal RevenueLifetime { get; set; }
        public decimal CollectedLifetime { get; set; }
        public decimal OutstandingTotal { get; set; }
    }

    public class TutorReportDto
    {
        public ReportSummaryDto Summary { get; set; } = new();
        /// <summary>6 tháng gần nhất, oldest → newest.</summary>
        public List<MonthlyPointDto> Months { get; set; } = new();
        public List<TopStudentDto> TopStudents { get; set; } = new();
    }
}
