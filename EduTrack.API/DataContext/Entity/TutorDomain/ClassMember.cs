using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext.Entity.TutorDomain
{
    /// <summary>
    /// Ghi danh: HS thuộc lớp nào. 1 HS có thể học nhiều lớp (Tâm học Toán 1A + Toán 1B).
    /// Giá buổi của em trong lớp = RateOverride ?? ClassRoom.DefaultRatePerLesson.
    /// Soft-delete = rời lớp (buổi đã sinh không bị ảnh hưởng).
    /// </summary>
    public class ClassMember : BaseEntity, ITutorScoped
    {
        public Guid IdTutor { get; set; }

        public Guid IdClass { get; set; }

        public Guid IdStudent { get; set; }

        /// <summary>Giá riêng cho em đặc biệt — null = theo giá lớp.</summary>
        public decimal? RateOverride { get; set; }
    }
}
