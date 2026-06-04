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
    [TDModule("Học sinh", 20)]
    public class StudentController : ApiController
    {
        private readonly IStudentService _service;
        public StudentController(IStudentService service) { _service = service; }

        [TDPermission("GetByFilterAsync", "Danh sách học sinh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<StudentDetailDto>>>>> GetByFilterAsync(StudentGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("GetByIdAsync", "Xem học sinh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-id/{id:Guid}")]
        public async Task<ActionResult<Response<StudentDetailDto>>> GetByIdAsync(Guid id)
            => Ok(await _service.GetDetailByIdAsync(id));

        [TDPermission("CreateAsync", "Thêm học sinh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<StudentDto>>> CreateAsync(StudentDto dto)
        {
            // Tạo HS + seed môn học đầu tiên (Subject + FirstCourseRate) trong 1 phát
            return Ok(await _service.CreateWithFirstCourseAsync(dto));
        }

        [TDPermission("UpdateAsync", "Sửa học sinh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update")]
        public async Task<ActionResult<Response<StudentDto>>> UpdateAsync(StudentDto dto)
        {
            var entity = AutoMapperGeneric.Map<StudentDto, Student>(dto);
            return Ok(await _service.UpdateAsync(entity));
        }

        [TDPermission("DeleteAsync", "Xoá học sinh", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<StudentDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));
    }
}
