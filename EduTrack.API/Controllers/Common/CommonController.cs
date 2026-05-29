using EduTrack.API.Controllers.Base;
using EduTrack.API.Services.Common;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : ApiController
    {
        private readonly ILogger<CommonController> _logger;
        private readonly ICommonService _commonService;

        public CommonController(ILogger<CommonController> logger, ICommonService commonService)
        {
            _logger = logger;
            _commonService = commonService;
        }

        [Route("dropdown-page")]
        [HttpGet]
        public async Task<ActionResult<Response<List<Dropdown>>>> DropdownPageAsync()
        {
            return Ok(await _commonService.DropdownPageAsync());
        }

        [Route("dropdown-user")]
        [HttpGet]
        public async Task<ActionResult<Response<List<Dropdown>>>> DropdownUserAsync()
        {
            return Ok(await _commonService.DropdownUserAsync());
        }

        [Route("dropdown-role")]
        [HttpGet]
        public async Task<ActionResult<Response<List<Dropdown>>>> DropdownRoleAsync()
        {
            return Ok(await _commonService.DropdownRoleAsync());
        }
    }
}
