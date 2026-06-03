namespace EduTrack.API.Commons
{
    /// <summary>
    /// Nguồn sự thật DUY NHẤT về thời gian + múi giờ cho toàn app. Mọi quy đổi VN↔UTC
    /// phải đi qua đây — KHÔNG hardcode +7/-7 rải rác.
    ///
    /// Quy ước (đọc kỹ trước khi thêm field thời gian mới):
    /// <list type="bullet">
    ///   <item><b>INSTANT</b> (thời điểm một sự kiện xảy ra / sẽ xảy ra): luôn lưu <b>UTC</b> bằng
    ///   <see cref="UtcNow"/>. VD: audit (DateCreated…), Notification.ScheduledAt/SentAt,
    ///   Lesson.DoneAt, Subscription.*, payment.ConfirmedAt.</item>
    ///   <item><b>CIVIL TIME</b> (giờ dân sự VN — vd "buổi học 19:30"): lưu nguyên giờ VN
    ///   (Lesson.ScheduledDate + StartTime/EndTime). KHÔNG đổi sang UTC vì nó là "19:30 Việt Nam"
    ///   theo định nghĩa, không phụ thuộc người xem.</item>
    ///   <item>Khi cần <b>instant của một civil time</b> (lên lịch nhắc, so với "bây giờ"):
    ///   <see cref="VnToUtc"/>. Khi cần <b>lịch VN từ một instant</b> (ngày/tháng/giờ dân sự hiện tại):
    ///   <see cref="UtcToVn"/> / <see cref="VnNow"/>.</item>
    /// </list>
    ///
    /// VN không có DST → offset cố định +7. Dùng custom <see cref="TimeZoneInfo"/> (không phụ thuộc
    /// tz database của OS) để chạy đồng nhất trên Windows/Linux.
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
