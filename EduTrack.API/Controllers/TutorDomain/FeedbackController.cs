using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Góp ý", 50)]
    public class FeedbackController : ApiController
    {
        private readonly IFeedbackService _service;
        public FeedbackController(IFeedbackService service) { _service = service; }

        // ─── Tutor ───
        [TDPermission("CreateMyAsync", "Gửi góp ý", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<FeedbackDto>>> CreateMyAsync(FeedbackDto dto)
            => Ok(await _service.CreateMyAsync(dto));

        [TDPermission("GetMineAsync", "Góp ý của tôi", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-mine")]
        public async Task<ActionResult<Response<PagingData<List<FeedbackDto>>>>> GetMineAsync(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Ok(await _service.GetMineAsync(pageNumber, pageSize));

        // ─── Founder (IsSuper bypass — không gán role nào) ───
        [TDPermission("GetByFilterAsync", "Danh sách góp ý (admin)")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<FeedbackDetailDto>>>>> GetByFilterAsync(FeedbackGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("UpdateStatusAsync", "Phản hồi góp ý (admin)")]
        [TDAuthorize]
        [HttpPost("update-status/{id:Guid}")]
        public async Task<ActionResult<Response<FeedbackDto>>> UpdateStatusAsync(Guid id, [FromBody] FeedbackUpdateStatusReq req)
            => Ok(await _service.UpdateStatusAsync(id, req));

        [TDPermission("GetNewCountAsync", "Số góp ý mới (admin)")]
        [TDAuthorize]
        [HttpGet("new-count")]
        public async Task<ActionResult<Response<int>>> GetNewCountAsync()
            => Ok(await _service.GetNewCountAsync());
    }
}
