using EduTrack.API.Commons;
using EduTrack.API.DataContext;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Jobs
{
    /// <summary>
    /// Background job duy trì sức khỏe table Notifications:
    /// <list type="bullet">
    ///   <item>Auto-email phụ huynh CÓ EMAIL khi nhắc Pending đến hạn (kênh phụ — không đổi Status).</item>
    ///   <item>Mark Pending quá hạn 72h sau ScheduledAt → Missed (để inbox không treo nhắc cũ mãi).</item>
    ///   <item>Soft-delete Sent / Cancelled / Missed cũ hơn 60 ngày (cleanup).</item>
    ///   <item>(Tuỳ chọn) Daily digest email 7:00 ICT cho tutor có ≥1 nhắc Pending due.</item>
    /// </list>
    /// Chạy mỗi <see cref="TickInterval"/>. KHÔNG gửi Zalo (đó là việc tutor làm tay trong /inbox).
    /// </summary>
    public class NotificationDispatcherJob : BackgroundService
    {
        private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(5);
        private const int MissedAfterHours = 72;
        private const int CleanupAfterDays = 60;
        /// <summary>Giờ Vietnam (ICT = UTC+7) gửi daily digest.</summary>
        private const int DigestHourIct = 7;

        private readonly IServiceProvider _sp;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationDispatcherJob> _logger;
        private DateOnly? _lastDigestSentDateIct;

        public NotificationDispatcherJob(IServiceProvider sp, IConfiguration config, ILogger<NotificationDispatcherJob> logger)
        {
            _sp = sp;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("🔔 NotificationDispatcherJob started (interval {Min}m)", TickInterval.TotalMinutes);
            // Delay nhẹ để app khởi động xong DB pool / EF cache trước
            await Task.Delay(TimeSpan.FromSeconds(30), ct);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await TickAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NotificationDispatcherJob tick lỗi — sẽ thử lại sau {Min}m", TickInterval.TotalMinutes);
                }

                try { await Task.Delay(TickInterval, ct); }
                catch (TaskCanceledException) { break; }
            }
        }

        private async Task TickAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EduTrackDbContext>();
            var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = AppTime.VnNow;

            // ── 0) Auto-email phụ huynh có email — kênh phụ, KHÔNG đổi Status ──
            await TryAutoEmailParentsAsync(db, email, now, ct);

            // ── 1) Mark Missed: Pending quá ScheduledAt + 72h ──
            var missedCutoff = now.AddHours(-MissedAfterHours);
            var missed = await db.Notifications
                .Where(n => n.Status == NotificationStatusEnums.Pending && n.ScheduledAt < missedCutoff)
                .ToListAsync(ct);
            foreach (var n in missed)
            {
                n.Status = NotificationStatusEnums.Missed;
                n.MarkDirty(nameof(n.Status));
            }
            if (missed.Count > 0)
                _logger.LogInformation("⏰ Marked {Count} notification(s) Missed (quá {Hours}h)", missed.Count, MissedAfterHours);

            // ── 2) Cleanup notification cũ ──
            var cleanupCutoff = now.AddDays(-CleanupAfterDays);
            var oldOnes = await db.Notifications
                .Where(n => (n.Status == NotificationStatusEnums.Sent
                          || n.Status == NotificationStatusEnums.Cancelled
                          || n.Status == NotificationStatusEnums.Missed
                          || n.Status == NotificationStatusEnums.Read)
                         && n.DateCreated < cleanupCutoff)
                .ToListAsync(ct);
            foreach (var n in oldOnes)
            {
                n.IsDeleted = true;
                n.DateDeleted = now;
                n.MarkDirty(nameof(n.IsDeleted));
                n.MarkDirty(nameof(n.DateDeleted));
            }
            if (oldOnes.Count > 0)
                _logger.LogInformation("🧹 Soft-deleted {Count} notification(s) cũ hơn {Days}d", oldOnes.Count, CleanupAfterDays);

            if (missed.Count > 0 || oldOnes.Count > 0)
                await db.SaveChangesAsync(ct);

            // ── 3) Daily digest email — 7:00 ICT, 1 lần/ngày ──
            await TrySendDailyDigestAsync(db, email, now, ct);
        }

        /// <summary>
        /// Gửi email nội dung nhắc cho phụ huynh CÓ EMAIL khi nhắc Pending đến hạn.
        /// Kênh phụ bên cạnh Zalo tay: KHÔNG đổi Status (tutor vẫn thấy "Chờ gửi" để Zalo);
        /// chỉ stamp EmailedAt để không gửi lặp. Lỗi gửi 1 item không chặn các item khác.
        /// </summary>
        private async Task TryAutoEmailParentsAsync(EduTrackDbContext db, IEmailService email, DateTime now, CancellationToken ct)
        {
            const int batchSize = 50; // an toàn SMTP — phần dư sẽ đi ở tick sau

            var due = await (
                from n in db.Notifications
                where n.Status == NotificationStatusEnums.Pending
                   && n.ScheduledAt <= now
                   && n.EmailedAt == null
                   && n.IdStudent != null
                join s in db.Students on n.IdStudent equals s.Id
                join p in db.Parents on s.IdParent equals p.Id
                where p.Email != null && p.Email != ""
                join u in db.Users on n.IdTutor equals u.Id
                select new { Noti = n, ParentEmail = p.Email!, ParentName = p.FullName, TutorName = u.DisplayName }
            ).Take(batchSize).ToListAsync(ct);

            if (due.Count == 0) return;

            int sentCount = 0;
            foreach (var item in due)
            {
                var html = $@"
<div style='font-family:Inter,sans-serif;max-width:520px;margin:0 auto;padding:28px;background:#F7F8FC;color:#1A1F36'>
  <p style='white-space:pre-line;margin:0 0 20px'>{System.Net.WebUtility.HtmlEncode(item.Noti.BodyText)}</p>
  <hr style='border:none;border-top:1px solid #E3E6F0;margin:20px 0'/>
  <p style='color:#8A91A8;font-size:12px;margin:0'>
    Tin nhắc tự động từ EduTrack thay mặt gia sư {System.Net.WebUtility.HtmlEncode(item.TutorName)}.
  </p>
</div>";
                var ok = await email.SendAsync(item.ParentEmail, item.Noti.Title, html, item.Noti.BodyText);
                if (ok)
                {
                    item.Noti.EmailedAt = now;
                    item.Noti.MarkDirty(nameof(item.Noti.EmailedAt));
                    sentCount++;
                }
                // ok=false → để EmailedAt null, tick sau retry (EmailService đã log lỗi)
            }

            if (sentCount > 0)
            {
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("📧 Auto-emailed {Count} nhắc cho phụ huynh", sentCount);
            }
        }

        private async Task TrySendDailyDigestAsync(EduTrackDbContext db, IEmailService email, DateTime now, CancellationToken ct)
        {
            var nowIct = now;   // "now" đã là giờ VN
            if (nowIct.Hour < DigestHourIct) return;
            var todayIct = DateOnly.FromDateTime(nowIct);
            if (_lastDigestSentDateIct == todayIct) return;

            // Tutor có ≥1 nhắc Pending đã đến hạn
            var groups = await db.Notifications
                .Where(n => n.Status == NotificationStatusEnums.Pending && n.ScheduledAt <= now)
                .GroupBy(n => n.IdTutor)
                .Select(g => new { IdTutor = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            if (groups.Count == 0)
            {
                _lastDigestSentDateIct = todayIct;
                return;
            }

            var tutorIds = groups.Select(g => g.IdTutor).ToList();
            var tutors = await db.Users
                .Where(u => tutorIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                .Select(u => new { u.Id, u.Email, u.DisplayName })
                .ToListAsync(ct);

            var webUrl = _config["App:WebUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            foreach (var t in tutors)
            {
                var count = groups.First(g => g.IdTutor == t.Id).Count;
                var html = $@"
<div style='font-family:Inter,sans-serif;max-width:520px;margin:0 auto;padding:32px;background:#F7F8FC;color:#1A1F36'>
  <h2 style='color:#5B5FCF;margin:0 0 12px'>🔔 Bạn có {count} nhắc cần gửi hôm nay</h2>
  <p>Xin chào {t.DisplayName},</p>
  <p>Có {count} tin nhắc phụ huynh đang chờ trong Hộp nhắc EduTrack — mở app và forward Zalo trong vài giây.</p>
  <p style='text-align:center;margin:28px 0'>
    <a href='{webUrl}/inbox' style='background:linear-gradient(135deg,#5B5FCF,#7C3AED);color:#fff;padding:14px 28px;border-radius:8px;text-decoration:none;font-weight:700;display:inline-block'>Mở Hộp nhắc</a>
  </p>
</div>";
                await email.SendAsync(t.Email!, $"EduTrack — {count} nhắc cần gửi hôm nay", html);
            }
            _logger.LogInformation("📬 Daily digest sent to {Count} tutor(s)", tutors.Count);
            _lastDigestSentDateIct = todayIct;
        }
    }
}
