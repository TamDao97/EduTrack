namespace EduTrack.API.Commons
{
    /// <summary>
    /// Nguồn sự thật DUY NHẤT về thời gian + múi giờ cho toàn app. KHÔNG hardcode +7/-7 rải rác.
    ///
    /// QUY ƯỚC (app chỉ phục vụ VN → lưu giờ VN local cho tất cả):
    /// <list type="bullet">
    ///   <item>Mọi field thời gian lưu DB là <b>giờ VN (civil local)</b>: dùng <see cref="VnNow"/>
    ///   khi gán "bây giờ". VD: audit (DateCreated…), Notification.ScheduledAt/SentAt,
    ///   Lesson.DoneAt/ScheduledDate, Subscription.*, payment.ConfirmedAt.</item>
    ///   <item>Mọi chỗ <b>so sánh với "bây giờ"</b> cũng dùng <see cref="VnNow"/> → DB-VN so với
    ///   now-VN, cùng hệ, không lệch.</item>
    /// </list>
    ///
    /// <see cref="UtcNow"/> / <see cref="VnToUtc"/> / <see cref="UtcToVn"/> chỉ dùng khi cần
    /// interop UTC với bên ngoài (vd token, hệ thống khác). VN không có DST → offset cố định +7,
    /// dùng custom <see cref="TimeZoneInfo"/> (không phụ thuộc tz database của OS).
    /// </summary>
    public static class AppTime
    {
        public static readonly TimeZoneInfo VietnamZone =
            TimeZoneInfo.CreateCustomTimeZone("Asia/Ho_Chi_Minh", TimeSpan.FromHours(7), "Vietnam (UTC+7)", "ICT");

        /// <summary>Thời điểm hiện tại (UTC) — dùng cho mọi instant lưu DB.</summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>"Bây giờ" theo lịch VN — dùng khi cần ngày/tháng/giờ dân sự hiện tại.</summary>
        public static DateTime VnNow => UtcToVn(DateTime.UtcNow);

        /// <summary>Quy một civil time VN → UTC instant.</summary>
        public static DateTime VnToUtc(DateTime vnCivil)
            => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(vnCivil, DateTimeKind.Unspecified), VietnamZone);

        /// <summary>Quy một UTC instant → giờ dân sự VN.</summary>
        public static DateTime UtcToVn(DateTime utc)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), VietnamZone);
    }
}
