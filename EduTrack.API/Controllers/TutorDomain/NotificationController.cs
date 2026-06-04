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
    [TDModule("Nhắc gửi phụ huynh", 35)]
    public class NotificationController : ApiController
    {
        private readonly INotificationService _service;
        public NotificationController(INotificationService service) { _service = service; }

        [TDPermission("GetInboxAsync", "Hộp nhắc cần gửi", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-inbox")]
        public async Task<ActionResult<Response<PagingData<List<NotificationDto>>>>> GetInboxAsync(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
            => Ok(await _service.GetInboxAsync(pageNumber, pageSize));

        [TDPermission("GetByFilterAsync", "Danh sách nhắc", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<NotificationDto>>>>> GetByFilterAsync(NotificationGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("MarkSentAsync", "Đánh dấu đã gửi", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("mark-sent/{id:Guid}")]
        public async Task<ActionResult<Response<NotificationDto>>> MarkSentAsync(Guid id)
            => Ok(await _service.MarkSentAsync(id));

        [TDPermission("GetDueCountAsync", "Số nhắc đến hạn", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("due-count")]
        public async Task<ActionResult<Response<int>>> GetDueCountAsync()
            => Ok(await _service.GetDueCountAsync());
    }
}
