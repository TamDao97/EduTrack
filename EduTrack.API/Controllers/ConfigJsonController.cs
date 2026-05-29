using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Cài đặt cấu hình chung", 1)]
    public class ConfigJsonController : ApiController
    {
        private readonly ILogger<ConfigJsonController> _logger;
        private readonly IConfigJsonService _configJsonService;

        public ConfigJsonController(ILogger<ConfigJsonController> logger, IConfigJsonService configJsonService)
        {
            _logger = logger;
            _configJsonService = configJsonService;
        }

        [TDPermission("GetOrgConfigAsync", "Xem cấu hình chung đơn vị", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("get-org-config")]
        [HttpGet]
        public async Task<ActionResult<Response<JsonOrgObjectDto>>> GetOrgConfigAsync()
        {
            return Ok(await _configJsonService.GetOrgConfigAsync());
        }

        [TDPermission("SaveOrgConfigAsync", "Lưu cấu hình chung đơn vị", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("save-org-config")]
        [HttpPost]
        public async Task<ActionResult<Response<JsonOrgObjectDto>>> SaveOrgConfigAsync(JsonOrgObjectDto dtoReq)
        {
            return Ok(await _configJsonService.SaveOrgConfigAsync(dtoReq));
        }
    }
}
