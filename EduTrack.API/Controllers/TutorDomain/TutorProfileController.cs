using EduTrack.API.Attributes;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Hồ sơ gia sư", 10)]
    public class TutorProfileController : ApiController
    {
        private readonly ITutorProfileService _service;
        public TutorProfileController(ITutorProfileService service) { _service = service; }

        [TDAuthorize]
        [HttpGet("get-my-profile")]
        public async Task<ActionResult<Response<TutorProfileDto>>> GetMyProfileAsync()
            => Ok(await _service.GetMyProfileAsync());

        [TDAuthorize]
        [HttpPost("save-my-profile")]
        public async Task<ActionResult<Response<TutorProfileDto>>> SaveMyProfileAsync(TutorProfileDto dto)
            => Ok(await _service.SaveMyProfileAsync(dto));
    }
}
