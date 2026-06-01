using System.Net;
using System.Net.Mail;

namespace EduTrack.API.Services.Common
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email. Nếu SMTP chưa cấu hình (Enabled=false hoặc host trống) thì
        /// log ra console + trả về true để dev flow không vỡ. KHÔNG throw.
        /// </summary>
        Task<bool> SendAsync(string toAddress, string subject, string htmlBody, string? plainBody = null);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toAddress, string subject, string htmlBody, string? plainBody = null)
        {
            var smtp = _config.GetSection("Smtp");
            var enabled = smtp.GetValue<bool>("Enabled");
            var host = smtp.GetValue<string>("Host");
            var user = smtp.GetValue<string>("User");
            var fromAddress = smtp.GetValue<string>("FromAddress") ?? "no-reply@localhost";
            var fromName = smtp.GetValue<string>("FromName") ?? "EduTrack";

            // Dev mode — log only, don't fail.
            if (!enabled || string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning(
                    "📧 [SMTP disabled] Email tới {To}\n  Subject: {Subject}\n  Body: {Body}",
                    toAddress, subject, plainBody ?? StripHtml(htmlBody));
                return true;
            }

            try
            {
                using var client = new SmtpClient(host, smtp.GetValue<int>("Port"))
                {
                    EnableSsl = smtp.GetValue<bool>("UseSsl"),
                    Credentials = string.IsNullOrEmpty(user)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(user, smtp.GetValue<string>("Password")),
                };

                using var msg = new MailMessage
                {
                    From = new MailAddress(fromAddress, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                };
                msg.To.Add(toAddress);
                await client.SendMailAsync(msg);
                _logger.LogInformation("✉️ Email sent to {To}: {Subject}", toAddress, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gửi tới {To} thất bại", toAddress);
                return false;
            }
        }

        private static string StripHtml(string html)
            => System.Text.RegularExpressions.Regex.Replace(html ?? "", "<[^>]*>", "");
    }
}
