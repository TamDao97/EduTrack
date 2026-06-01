using System.ComponentModel.DataAnnotations;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Hồ sơ mở rộng của gia sư (1:1 với User có role=TUTOR).
    /// Lưu thông tin ngân hàng để sinh text/QR thu học phí + bio public.
    /// </summary>
    public class TutorProfile : BaseEntity
    {
        public Guid IdUser { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(200)]
        public string? BankAccountHolder { get; set; }

        /// <summary>Danh sách môn dạy, cách nhau dấu `;` (vd: "Toán;Lý;Hóa").</summary>
        [MaxLength(500)]
        public string? Subjects { get; set; }

        public string? Bio { get; set; }

        /// <summary>Path tới ảnh đại diện đã upload qua FileService (vd: "/uploads/images/abc.png").</summary>
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
    }
}
