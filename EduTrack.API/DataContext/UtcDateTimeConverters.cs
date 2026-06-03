using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EduTrack.API.DataContext
{
    /// <summary>
    /// Ép <see cref="DateTimeKind.Utc"/> khi đọc DateTime từ DB (datetime2 trả về Kind=Unspecified).
    /// Nhờ vậy System.Text.Json serialize kèm hậu tố 'Z' → client tự localize đúng theo múi giờ.
    /// Áp cho MỌI instant; civil time (Lesson.ScheduledDate) được loại trừ ở OnModelCreating.
    /// Giá trị KHÔNG đổi (chỉ gắn Kind), nên không ảnh hưởng so sánh/index.
    /// </summary>
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }

    public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter()
            : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
    }
}
