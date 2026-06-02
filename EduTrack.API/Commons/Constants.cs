namespace EduTrack.API.Commons
{
    public class Constants
    {
    }

    public class AppSettings
    {
        public Jwt Jwt { get; set; }
        public string? TemplateFolder { get; set; }
    }

    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    public class RoleCodes
    {
        // Founder/super KHÔNG dùng role — định danh bằng cờ User.IsSuper (bypass [TDPermission]).
        // Giữ lại Admin chỉ để tính cờ User.IsAdmin (xem UserService); Tutor là role nghiệp vụ thật.
        public const string Admin = "ADMIN";
        public const string Tutor = "TUTOR";
    }
}
