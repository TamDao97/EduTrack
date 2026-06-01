using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.Core
{
    /// <summary>
    /// Token reset password 1-lần. Lifetime mặc định 1h. Đánh dấu UsedAt sau khi tutor
    /// hoàn tất reset → token không thể tái sử dụng. Tạo token mới sẽ vô hiệu hoá các
    /// token cũ chưa dùng (xử lý ở service).
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        public Guid IdUser { get; set; }

        /// <summary>Token random 32 ký tự — query bằng plain text (không hash để đơn giản MVP).</summary>
        [Required, MaxLength(64)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
