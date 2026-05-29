using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Enums;
using TD.Lib.Common;

namespace EduTrack.API.DataContext.Dto.Core
{
    public class UserDto : BaseDto
    {
        public string? UserName { get; set; }
        public string DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTimeOffset? DateLockout { get; set; }
        public bool IsLockout { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsSuper { get; set; } = false;
        public GenderEnums? Gender { get; set; }
    }

    public class UserCreateReqDto : UserDto
    {
        public string? Password { get; set; }
        public List<Guid> LstIdRole { get; set; } = new List<Guid>();
    }

    public class UserCreateResDto : UserDto
    {
        public List<Guid> LstIdRole { get; set; } = new List<Guid>();
    }

    public class UserUpdateReqDto : UserCreateReqDto
    {

    }

    public class UserUpdateResDto : UserCreateResDto
    {

    }

    public class UserProfileSaveReq : UserDto
    {

    }

    public class UserChangePasswordReq
    {
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }

    public class UserGridFilter : GridFilterBase
    {
    }
}
