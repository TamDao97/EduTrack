using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Web;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    /// <summary>
    /// Sinh Notification reactive cho mỗi nghiệp vụ (Lesson, TuitionPeriod).
    /// Tách riêng để LessonService / TuitionPeriodService chỉ gọi 1 method, không lo logic nội dung.
    /// </summary>
    public interface INotificationGenerator
    {
        Task GenerateForLessonAsync(Lesson lesson);
        Task CancelForLessonAsync(Guid idLesson);
        Task GenerateForTuitionPeriodAsync(TuitionPeriod period);
    }

    public class NotificationGenerator : INotificationGenerator
    {
        private readonly IUnitOfWork _uow;
        private readonly ITDRepository<Notification> _notiRepos;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Parent> _parentRepos;
        private readonly ITDRepository<TutorProfile> _profileRepos;

        public NotificationGenerator(IUnitOfWork uow)
        {
            _uow = uow;
            _notiRepos = uow.GetRepository<Notification>();
            _studentRepos = uow.GetRepository<Student>();
            _parentRepos = uow.GetRepository<Parent>();
            _profileRepos = uow.GetRepository<TutorProfile>();
        }

        public async Task GenerateForLessonAsync(Lesson lesson)
        {
            var (student, parent) = await GetStudentParentAsync(lesson.IdStudent);
            if (parent == null) return; // không có phụ huynh thì không sinh nhắc

            var lessonStart = lesson.ScheduledDate.Date.Add(lesson.StartTime);

            // 2 mốc nhắc (giờ buổi học nhập theo VN(+7) → quy về UTC để ScheduledAt khớp với
            // DateTime.UtcNow dùng ở GetInbox + NotificationDispatcherJob): tối hôm trước 19h + 1h trước buổi.
            var eveningBefore = VnToUtc(lesson.ScheduledDate.Date.AddDays(-1).AddHours(19));
            var hourBefore = VnToUtc(lessonStart.AddHours(-1));

            var dayName = LocalizeDayOfWeek(lesson.ScheduledDate.DayOfWeek);
            var dateStr = lesson.ScheduledDate.ToString("dd/MM/yyyy");
            var timeStr = $"{lesson.StartTime:hh\\:mm} - {lesson.EndTime:hh\\:mm}";

            var title = $"{student?.FullName ?? "Học sinh"} · {dayName} {dateStr} · {timeStr}";

            var bodyEvening = BuildLessonReminderText(student?.FullName, parent.FullName, dayName, dateStr, timeStr, lesson.Location, isHourBefore: false);
            var bodyHour = BuildLessonReminderText(student?.FullName, parent.FullName, dayName, dateStr, timeStr, lesson.Location, isHourBefore: true);

            var notis = new List<Notification>
            {
                BuildNoti(lesson, student, parent, NotificationTypeEnums.LessonReminderEvening, title, bodyEvening, eveningBefore),
                BuildNoti(lesson, student, parent, NotificationTypeEnums.LessonReminderHourBefore, title, bodyHour, hourBefore),
            };

            await _notiRepos.CreateMultiAsync(notis);
            await _uow.SaveChangesAsync();
        }

        public async Task CancelForLessonAsync(Guid idLesson)
        {
            var pending = await _notiRepos.Table
                .Where(n => n.RefId == idLesson
                         && (n.Type == NotificationTypeEnums.LessonReminderEvening
                          || n.Type == NotificationTypeEnums.LessonReminderHourBefore)
                         && n.Status == NotificationStatusEnums.Pending)
                .ToListAsync();

            foreach (var n in pending)
            {
                n.Status = NotificationStatusEnums.Cancelled;
                n.MarkDirty(nameof(n.Status));
            }
            if (pending.Count > 0) await _uow.SaveChangesAsync();
        }

        public async Task GenerateForTuitionPeriodAsync(TuitionPeriod period)
        {
            var (student, parent) = await GetStudentParentAsync(period.IdStudent);
            if (parent == null) return;
            var profile = await _profileRepos.TableNoTracking
                .FirstOrDefaultAsync(p => p.IdUser == period.IdTutor);

            var title = $"Học phí {student?.FullName ?? ""} · T{period.PeriodMonth:00}/{period.PeriodYear}";
            var body = BuildTuitionText(student?.FullName, parent.FullName, period, profile);

            var noti = BuildNoti(
                refId: period.Id,
                idStudent: period.IdStudent,
                idTutor: period.IdTutor,
                parentPhone: parent.Phone,
                studentFullName: student?.FullName,
                type: NotificationTypeEnums.TuitionIssued,
                title: title,
                body: body,
                scheduledAt: DateTime.UtcNow
            );

            await _notiRepos.CreateAsync(noti);
            await _uow.SaveChangesAsync();
        }

        #region helpers
        /// <summary>VN không có DST → offset cố định +7. Quy giờ VN (wall-clock buổi học) về UTC.</summary>
        private const int VnOffsetHours = 7;
        private static DateTime VnToUtc(DateTime vn) => vn.AddHours(-VnOffsetHours);

        private async Task<(Student? student, Parent? parent)> GetStudentParentAsync(Guid idStudent)
        {
            var student = await _studentRepos.TableNoTracking.FirstOrDefaultAsync(s => s.Id == idStudent);
            if (student == null) return (null, null);
            var parent = await _parentRepos.TableNoTracking.FirstOrDefaultAsync(p => p.Id == student.IdParent);
            return (student, parent);
        }

        private Notification BuildNoti(Lesson lesson, Student? s, Parent p, NotificationTypeEnums type, string title, string body, DateTime scheduledAt)
            => BuildNoti(lesson.Id, lesson.IdStudent, lesson.IdTutor, p.Phone, s?.FullName, type, title, body, scheduledAt);

        private Notification BuildNoti(Guid refId, Guid idStudent, Guid idTutor, string? parentPhone, string? studentFullName,
            NotificationTypeEnums type, string title, string body, DateTime scheduledAt) => new()
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                IdStudent = idStudent,
                Type = type,
                RefId = refId,
                Title = title,
                BodyText = body,
                ParentPhone = parentPhone,
                StudentFullName = studentFullName,
                ZaloDeepLink = BuildZaloDeepLink(parentPhone, body),
                Status = NotificationStatusEnums.Pending,
                ScheduledAt = scheduledAt,
            };

        private static string? BuildZaloDeepLink(string? phone, string text)
        {
            if (string.IsNullOrEmpty(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return $"https://zalo.me/{digits}?text={HttpUtility.UrlEncode(text)}";
        }

        private static string LocalizeDayOfWeek(DayOfWeek d) => d switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            _ => "Chủ nhật",
        };

        private static string BuildLessonReminderText(string? studentName, string parentName, string day, string date, string time, string? location, bool isHourBefore)
        {
            var locationLine = string.IsNullOrEmpty(location) ? "" : $"\nĐịa điểm: {location}";
            var when = isHourBefore ? "1 giờ nữa" : "ngày mai";
            return $"Em chào {parentName}!\n" +
                   $"Em xin nhắc lịch học của {studentName} {when}:\n" +
                   $"• {day} {date}\n" +
                   $"• {time}{locationLine}\n\n" +
                   $"Phụ huynh sắp xếp giúp em ạ. Em cám ơn!";
        }

        private static string BuildTuitionText(string? studentName, string parentName, TuitionPeriod period, TutorProfile? profile)
        {
            var bankLine = profile != null && !string.IsNullOrEmpty(profile.BankAccountNumber)
                ? $"\n\nThông tin chuyển khoản:\n• Ngân hàng: {profile.BankName}\n• Số TK: {profile.BankAccountNumber}\n• Chủ TK: {profile.BankAccountHolder}\n• Nội dung: HP {studentName} T{period.PeriodMonth:00}"
                : "";

            return $"Em chào {parentName}!\n" +
                   $"Em gửi học phí tháng {period.PeriodMonth:00}/{period.PeriodYear} của {studentName}:\n" +
                   $"• Số buổi đã dạy: {period.TotalLessons}\n" +
                   $"• Học phí: {period.TotalAmount.ToString("N0", CultureInfo.InvariantCulture)}đ" +
                   (period.Adjustment != 0 ? $"\n• Điều chỉnh: {period.Adjustment.ToString("N0", CultureInfo.InvariantCulture)}đ" : "") +
                   $"\n• Tổng: {period.FinalAmount.ToString("N0", CultureInfo.InvariantCulture)}đ" +
                   bankLine +
                   $"\n\nEm cám ơn phụ huynh!";
        }
        #endregion
    }
}
