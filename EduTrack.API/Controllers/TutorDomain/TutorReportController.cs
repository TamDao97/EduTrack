using EduTrack.API.Attributes;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    /// <summary>Báo cáo nhanh cho tutor — số liệu 6 tháng + lifetime + top HS.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Báo cáo", 40)]
    public class TutorReportController : ApiController
    {
        private readonly IReportService _service;
        public TutorReportController(IReportService service) { _service = service; }

        [TDAuthorize]
        [HttpGet("get-my-report")]
        public async Task<ActionResult<Response<TutorReportDto>>> GetMyReportAsync()
            => Ok(await _service.GetMyReportAsync());
    }
}
