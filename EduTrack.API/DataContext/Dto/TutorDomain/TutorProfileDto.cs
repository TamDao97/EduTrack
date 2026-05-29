using EduTrack.API.DataContext.Dto.Base;

namespace EduTrack.API.DataContext.Dto.TutorDomain
{
    public class TutorProfileDto : BaseDto
    {
        public Guid IdUser { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? Subjects { get; set; }
        public string? Bio { get; set; }
    }
}
