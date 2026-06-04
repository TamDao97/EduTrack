using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.AutoMapper;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Buổi học", 30)]
    public class LessonController : ApiController
    {
        private readonly ILessonService _service;
        public LessonController(ILessonService service) { _service = service; }

        [TDPermission("GetByFilterAsync", "Danh sách buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<LessonDetailDto>>>>> GetByFilterAsync(LessonGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("GetWeekAsync", "Lịch tuần", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-week")]
        public async Task<ActionResult<Response<List<LessonDetailDto>>>> GetWeekAsync([FromQuery] DateTime weekStart)
            => Ok(await _service.GetWeekAsync(weekStart));

        [TDPermission("GetByIdAsync", "Xem buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-id/{id:Guid}")]
        public async Task<ActionResult<Response<LessonDto>>> GetByIdAsync(Guid id)
            => Ok(await _service.GetByIdAsync(id));

        [TDPermission("CreateAsync", "Thêm 1 buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<LessonDto>>> CreateAsync(LessonDto dto)
        {
            var entity = AutoMapperGeneric.Map<LessonDto, Lesson>(dto);
            return Ok(await _service.CreateAsync(entity));
        }

        [TDPermission("BulkCreateRecurringAsync", "Thêm lịch lặp lại nhiều tuần", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("bulk-create-recurring")]
        public async Task<ActionResult<Response<int>>> BulkCreateRecurringAsync(LessonBulkCreateReq req)
            => Ok(await _service.BulkCreateRecurringAsync(req));

        [TDPermission("UpdateAsync", "Sửa buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update")]
        public async Task<ActionResult<Response<LessonDto>>> UpdateAsync(LessonDto dto)
        {
            var entity = AutoMapperGeneric.Map<LessonDto, Lesson>(dto);
            return Ok(await _service.UpdateAsync(entity));
        }

        [TDPermission("CreateGroupAsync", "Tạo buổi học nhóm", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create-group")]
        public async Task<ActionResult<Response<int>>> CreateGroupAsync(LessonGroupCreateReq req)
            => Ok(await _service.CreateGroupAsync(req));

        [TDPermission("MarkDoneAsync", "Đánh dấu đã dạy", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("mark-done/{id:Guid}")]
        public async Task<ActionResult<Response<LessonDto>>> MarkDoneAsync(Guid id)
            => Ok(await _service.MarkDoneAsync(id));

        [TDPermission("MarkDonePastAsync", "Đánh dấu đã dạy các buổi đã qua", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("mark-done-past")]
        public async Task<ActionResult<Response<int>>> MarkDonePastAsync()
            => Ok(await _service.MarkDonePastAsync());

        [TDPermission("CancelAsync", "Huỷ buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("cancel/{id:Guid}")]
        public async Task<ActionResult<Response<LessonDto>>> CancelAsync(Guid id, [FromBody] CancelReq req)
            => Ok(await _service.CancelAsync(id, req?.Reason));

        [TDPermission("DeleteAsync", "Xoá buổi học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<LessonDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));

        public class CancelReq { public string? Reason { get; set; } }
    }
}
