using EduTrack.API.DataContext.Dto.Base;

namespace EduTrack.API.DataContext.Dto
{
    public class ConfigJsonDto : BaseDto
    {
        public string? JsonOrg { get; set; }
        public string? JsonSupperAdmin { get; set; }
    }

    /// <summary>
    /// Cấu hình chung cho đơn vị — mở rộng tuỳ theo nghiệp vụ dự án.
    /// </summary>
    public class JsonOrgObjectDto
    {
        /// <summary>Tên hiển thị của ứng dụng</summary>
        public string? AppName { get; set; }
    }
}
