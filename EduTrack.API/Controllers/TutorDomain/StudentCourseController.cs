using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    // Route thật (kebab-case qua transformer): api/student-course
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Môn học của HS", 22)]
    public class StudentCourseController : ApiController
    {
        private readonly IStudentCourseService _service;
        public StudentCourseController(IStudentCourseService service) { _service = service; }

        [TDPermission("GetByStudentAsync", "Xem môn học của HS", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-student/{idStudent:Guid}")]
        public async Task<ActionResult<Response<List<StudentCourseDto>>>> GetByStudentAsync(Guid idStudent)
            => Ok(await _service.GetByStudentAsync(idStudent));

        [TDPermission("CreateCourseAsync", "Thêm môn học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<StudentCourseDto>>> CreateCourseAsync(StudentCourseDto dto)
            => Ok(await _service.CreateCourseAsync(dto));

        [TDPermission("UpdateCourseAsync", "Sửa môn học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update")]
        public async Task<ActionResult<Response<StudentCourseDto>>> UpdateCourseAsync(StudentCourseDto dto)
            => Ok(await _service.UpdateCourseAsync(dto));

        [TDPermission("DeleteAsync", "Xoá môn học", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<StudentCourseDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));
    }
}
