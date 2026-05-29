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
    [TDModule("Phụ huynh", 11)]
    public class ParentController : ApiController
    {
        private readonly IParentService _service;
        public ParentController(IParentService service) { _service = service; }

        [TDPermission("GetByFilterAsync", "Danh sách phụ huynh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<ParentDto>>>>> GetByFilterAsync(ParentGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("GetByIdAsync", "Xem phụ huynh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-id/{id:Guid}")]
        public async Task<ActionResult<Response<ParentDto>>> GetByIdAsync(Guid id)
            => Ok(await _service.GetByIdAsync(id));

        [TDPermission("CreateAsync", "Thêm phụ huynh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<ParentDto>>> CreateAsync(ParentDto dto)
        {
            var entity = AutoMapperGeneric.Map<ParentDto, Parent>(dto);
            return Ok(await _service.CreateAsync(entity));
        }

        [TDPermission("UpdateAsync", "Sửa phụ huynh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update")]
        public async Task<ActionResult<Response<ParentDto>>> UpdateAsync(ParentDto dto)
        {
            var entity = AutoMapperGeneric.Map<ParentDto, Parent>(dto);
            return Ok(await _service.UpdateAsync(entity));
        }

        [TDPermission("DeleteAsync", "Xoá phụ huynh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<ParentDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));
    }
}
