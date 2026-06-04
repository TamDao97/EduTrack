using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IReportService
    {
        Task<Response<TutorReportDto>> GetMyReportAsync();
    }

    /// <summary>
    /// Tính báo cáo cho tutor đang login: tổng quan + bar chart 6 tháng + top HS.
    /// Đọc no-tracking, tutor-scoped.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IUserContextService _userContext;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Lesson> _lessonRepos;
        private readonly ITDRepository<TuitionPeriod> _periodRepos;
        private readonly ITDRepository<StudentCourse> _courseRepos;

        public ReportService(IUnitOfWork uow, IUserContextService userContext)
        {
            _userContext = userContext;
            _studentRepos = uow.GetRepository<Student>();
            _lessonRepos = uow.GetRepository<Lesson>();
            _periodRepos = uow.GetRepository<TuitionPeriod>();
            _courseRepos = uow.GetRepository<StudentCourse>();
        }

        public async Task<Response<TutorReportDto>> GetMyReportAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Response<TutorReportDto>.Error(StatusCode.Unauthorized, "Chưa đăng nhập");
            var idTutor = cu.Id;

            // ── 6 tháng gần nhất (oldest → newest) — neo theo lịch VN ──
            var today = AppTime.VnNow;
            var anchors = new List<(int Year, int Month)>();
            var start = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
            for (int i = 0; i < 6; i++)
            {
                var m = start.AddMonths(i);
                anchors.Add((m.Year, m.Month));
            }
            var fromYear = anchors[0].Year;
            var fromMonth = anchors[0].Month;

            // Lessons done trong 6 tháng — group by year-month
            var lessonGroups = await _lessonRepos.TableNoTracking
                .Where(l => l.IdTutor == idTutor
                         && l.Status == LessonStatusEnums.Done
                         && (l.ScheduledDate.Year > fromYear
                          || (l.ScheduledDate.Year == fromYear && l.ScheduledDate.Month >= fromMonth)))
                .GroupBy(l => new { l.ScheduledDate.Year, l.ScheduledDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            // TuitionPeriod 6 tháng (kỳ đã chốt)
            var periodGroups = await _periodRepos.TableNoTracking
                .Where(p => p.IdTutor == idTutor
                         && p.ClosedAt != null
                         && (p.PeriodYear > fromYear
                          || (p.PeriodYear == fromYear && p.PeriodMonth >= fromMonth)))
                .GroupBy(p => new { p.PeriodYear, p.PeriodMonth })
                .Select(g => new
                {
                    Year = g.Key.PeriodYear,
                    Month = g.Key.PeriodMonth,
                    Revenue = g.Sum(x => x.FinalAmount),
                    Collected = g.Sum(x => x.PaidAmount),
                })
                .ToListAsync();

            var months = anchors.Select(a => new MonthlyPointDto
            {
                Year = a.Year,
                Month = a.Month,
                LessonsDone = lessonGroups.FirstOrDefault(g => g.Year == a.Year && g.Month == a.Month)?.Count ?? 0,
                Revenue = periodGroups.FirstOrDefault(g => g.Year == a.Year && g.Month == a.Month)?.Revenue ?? 0,
                Collected = periodGroups.FirstOrDefault(g => g.Year == a.Year && g.Month == a.Month)?.Collected ?? 0,
            }).ToList();

            // ── Summary lifetime ──
            var totalStudents = await _studentRepos.TableNoTracking
                .CountAsync(s => s.IdTutor == idTutor && s.Status == StudentStatusEnums.Active);

            var lessonsDoneLifetime = await _lessonRepos.TableNoTracking
                .CountAsync(l => l.IdTutor == idTutor && l.Status == LessonStatusEnums.Done);

            var lifetimePeriods = await _periodRepos.TableNoTracking
                .Where(p => p.IdTutor == idTutor && p.ClosedAt != null)
                .Select(p => new { p.FinalAmount, p.PaidAmount })
                .ToListAsync();
            var revenueLifetime = lifetimePeriods.Sum(p => p.FinalAmount);
            var collectedLifetime = lifetimePeriods.Sum(p => p.PaidAmount);

            // ── Top 5 students ──
            var topStudents = await (from p in _periodRepos.TableNoTracking
                                     join s in _studentRepos.TableNoTracking on p.IdStudent equals s.Id
                                     where p.IdTutor == idTutor && p.ClosedAt != null
                                     group new { p, s } by new { p.IdStudent, s.FullName } into g
                                     select new TopStudentDto
                                     {
                                         IdStudent = g.Key.IdStudent,
                                         FullName = g.Key.FullName,
                                         LessonsDone = g.Sum(x => x.p.TotalLessons),
                                         Revenue = g.Sum(x => x.p.FinalAmount),
                                     })
                .OrderByDescending(t => t.Revenue)
                .Take(5)
                .ToListAsync();

            // ── Doanh thu theo MÔN (6 tháng, theo ChargeAmount các buổi Done) ──
            var revenueBySubject = await (from l in _lessonRepos.TableNoTracking
                                          join c in _courseRepos.TableNoTracking on l.IdCourse equals c.Id into cj
                                          from c in cj.DefaultIfEmpty()
                                          where l.IdTutor == idTutor
                                             && l.Status == LessonStatusEnums.Done
                                             && (l.ScheduledDate.Year > fromYear
                                              || (l.ScheduledDate.Year == fromYear && l.ScheduledDate.Month >= fromMonth))
                                          group l by (c != null ? c.Subject : "Khác") into g
                                          select new SubjectRevenueDto
                                          {
                                              Subject = g.Key,
                                              LessonsDone = g.Count(),
                                              Amount = g.Sum(x => x.ChargeAmount),
                                          })
                .OrderByDescending(x => x.Amount)
                .ToListAsync();

            var rs = new TutorReportDto
            {
                Summary = new ReportSummaryDto
                {
                    TotalStudents = totalStudents,
                    LessonsDoneLifetime = lessonsDoneLifetime,
                    RevenueLifetime = revenueLifetime,
                    CollectedLifetime = collectedLifetime,
                    OutstandingTotal = revenueLifetime - collectedLifetime,
                },
                Months = months,
                TopStudents = topStudents,
                RevenueBySubject = revenueBySubject,
            };
            return Response<TutorReportDto>.Success(rs, StatusCode.Ok.ToDescription());
        }
    }
}
