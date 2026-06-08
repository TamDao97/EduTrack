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
        Task<Response<TutorReportDto>> GetMyReportAsync(int months = 6);
    }

    /// <summary>
    /// Tính báo cáo cho tutor đang login: tổng quan lifetime + bar chart / theo môn / top HS
    /// trong N tháng gần nhất (mặc định 6). Đọc no-tracking, tutor-scoped.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IUserContextService _userContext;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Lesson> _lessonRepos;
        private readonly ITDRepository<TuitionPeriod> _periodRepos;
        private readonly ITDRepository<StudentCourse> _courseRepos;
        private readonly ITDRepository<ClassRoom> _classRepos;

        public ReportService(IUnitOfWork uow, IUserContextService userContext)
        {
            _userContext = userContext;
            _studentRepos = uow.GetRepository<Student>();
            _lessonRepos = uow.GetRepository<Lesson>();
            _periodRepos = uow.GetRepository<TuitionPeriod>();
            _courseRepos = uow.GetRepository<StudentCourse>();
            _classRepos = uow.GetRepository<ClassRoom>();
        }

        public async Task<Response<TutorReportDto>> GetMyReportAsync(int months = 6)
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Response<TutorReportDto>.Error(StatusCode.Unauthorized, "Chưa đăng nhập");
            var idTutor = cu.Id;

            // ── N tháng gần nhất (oldest → newest) — neo theo lịch VN.
            // Clamp 1..24: đủ cho "Năm nay" lẫn 12 tháng, chặn kéo vô hạn dữ liệu.
            months = Math.Clamp(months, 1, 24);
            var today = AppTime.VnNow;
            var anchors = new List<(int Year, int Month)>();
            var start = new DateTime(today.Year, today.Month, 1).AddMonths(-(months - 1));
            for (int i = 0; i < months; i++)
            {
                var m = start.AddMonths(i);
                anchors.Add((m.Year, m.Month));
            }
            var fromYear = anchors[0].Year;
            var fromMonth = anchors[0].Month;

            // Lessons done trong khoảng xem — group by year-month
            var lessonGroups = await _lessonRepos.TableNoTracking
                .Where(l => l.IdTutor == idTutor
                         && l.Status == LessonStatusEnums.Done
                         && (l.ScheduledDate.Year > fromYear
                          || (l.ScheduledDate.Year == fromYear && l.ScheduledDate.Month >= fromMonth)))
                .GroupBy(l => new { l.ScheduledDate.Year, l.ScheduledDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            // TuitionPeriod trong khoảng xem (kỳ đã chốt)
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

            var monthPoints = anchors.Select(a => new MonthlyPointDto
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

            // ── Top 5 students — cùng khoảng xem với chart, không phải lifetime ──
            var topStudents = await (from p in _periodRepos.TableNoTracking
                                     join s in _studentRepos.TableNoTracking on p.IdStudent equals s.Id
                                     where p.IdTutor == idTutor && p.ClosedAt != null
                                        && (p.PeriodYear > fromYear
                                         || (p.PeriodYear == fromYear && p.PeriodMonth >= fromMonth))
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

            // ── Doanh thu theo MÔN (khoảng xem, theo ChargeAmount các buổi Done) ──
            // Môn: từ đăng ký 1-1 (StudentCourse) hoặc môn của Lớp (buổi sinh từ ClassRoom)
            var revenueBySubject = await (from l in _lessonRepos.TableNoTracking
                                          join c in _courseRepos.TableNoTracking on l.IdCourse equals c.Id into cj
                                          from c in cj.DefaultIfEmpty()
                                          join k in _classRepos.TableNoTracking on l.IdClass equals k.Id into kj
                                          from k in kj.DefaultIfEmpty()
                                          where l.IdTutor == idTutor
                                             && l.Status == LessonStatusEnums.Done
                                             && (l.ScheduledDate.Year > fromYear
                                              || (l.ScheduledDate.Year == fromYear && l.ScheduledDate.Month >= fromMonth))
                                          group l by (c != null ? c.Subject : (k != null && k.Subject != null ? k.Subject : "Khác")) into g
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
                Months = monthPoints,
                TopStudents = topStudents,
                RevenueBySubject = revenueBySubject,
            };
            return Response<TutorReportDto>.Success(rs, StatusCode.Ok.ToDescription());
        }
    }
}
