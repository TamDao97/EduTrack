using Microsoft.AspNetCore.Mvc;

namespace EduTrack.API.Controllers.Common
{
    /// <summary>
    /// Demo class — thay bằng DTO thực tế khi import.
    /// </summary>
    public class SampleExcelRow
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }

    [ApiController]
    [Route("api/excel")]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelService _excelService;

        public ExcelController(ExcelService excelService)
        {
            _excelService = excelService;
        }

        // ======================================
        // 1. DOWNLOAD TEMPLATE (demo)
        // ======================================
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var headers = new Dictionary<string, string>
            {
                { "Code", "Mã" },
                { "FullName", "Họ và Tên" },
                { "Age", "Tuổi" },
                { "Gender", "Giới tính" }
            };

            var bytes = _excelService.GenerateTemplateVer01(headers);

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SampleTemplate.xlsx");
        }

        // ======================================
        // 2. IMPORT FILE (demo)
        // ======================================
        [HttpPost("import")]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File không hợp lệ" });

            using var stream = file.OpenReadStream();

            var mapping = new Dictionary<string, string>
            {
                { "Mã", "Code" },
                { "Họ và Tên", "FullName" },
                { "Tuổi", "Age" },
                { "Giới tính", "Gender" }
            };

            var rules = new List<ExcelService.ImportRule>
            {
                new ExcelService.ImportRule { Property = "Code", Required = true, MaxLength = 20 },
                new ExcelService.ImportRule { Property = "FullName", Required = true },
                new ExcelService.ImportRule { Property = "Age", Required = true },
                new ExcelService.ImportRule { Property = "Gender", InList = new List<string>{ "Nam", "Nữ" } }
            };

            try
            {
                var result = _excelService.ImportWithMapping<SampleExcelRow>(stream, mapping, rules);

                if (result.HasError)
                {
                    return BadRequest(new
                    {
                        message = "Dữ liệu không hợp lệ",
                        errors = result.Errors
                    });
                }

                return Ok(new
                {
                    success = true,
                    total = result.Data.Count,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "File excel không hợp lệ hoặc bị hỏng",
                    detail = ex.Message
                });
            }
        }
    }
}
