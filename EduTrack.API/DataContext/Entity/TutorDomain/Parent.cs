using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Phụ huynh — chỉ là người nhận tin Zalo/Email, KHÔNG có account đăng nhập.
    /// 1 Parent có thể có nhiều Student (sibling).
    /// </summary>
    public class Parent : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>SĐT Zalo — dùng để sinh deeplink https://zalo.me/{phone}.</summary>
        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Notes { get; set; }
    }
}
