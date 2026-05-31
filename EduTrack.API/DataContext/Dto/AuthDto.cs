using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.API.DataContext.Dto
{
    public class LoginReq
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterReq
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thông tin!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thông tin!")]
        public string PasswordConfirm { get; set; }
    }

    /// <summary>Self-signup cho gia sư — tự động assign Role TUTOR + trả JWT.</summary>
    public class SignupTutorReq
    {
        [Required(ErrorMessage = "Vui lòng nhập email/username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Required]
        public string PasswordConfirm { get; set; }
    }

    public class CurrentUser
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string? Email { get; set; }
        public bool IsSuper { get; set; }
        public bool IsAdmin { get; set; }
        public string RoleName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
