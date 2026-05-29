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
        public const string SupperAdmin = "SUPPER_ADMIN";
        public const string Admin = "ADMIN";
        public const string User = "USER";
    }
}
