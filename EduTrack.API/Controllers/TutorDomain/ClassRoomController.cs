using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    // Route thật (kebab-case qua transformer): api/class-room
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Lớp học", 25)]
    public class ClassRoomController : ApiController
    {
        private readonly IClassRoomService _service;
        public ClassRoomController(IClassRoomService service) { _service = service; }

        [TDPermission("GetMyClassesAsync", "Danh sách lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-my-classes")]
        public async Task<ActionResult<Response<List<ClassRoomDto>>>> GetMyClassesAsync()
            => Ok(await _service.GetMyClassesAsync());

        [TDPermission("GetByFilterAsync", "Danh sách lớp (lọc + paging)", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<ClassRoomDto>>>>> GetByFilterAsync(ClassRoomGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("GetDetailAsync", "Chi tiết lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-detail/{id:Guid}")]
        public async Task<ActionResult<Response<ClassRoomDetailDto>>> GetDetailAsync(Guid id)
            => Ok(await _service.GetDetailAsync(id));

        [TDPermission("CreateClassAsync", "Tạo lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("create")]
        public async Task<ActionResult<Response<ClassRoomDto>>> CreateClassAsync(ClassRoomDto dto)
            => Ok(await _service.CreateClassAsync(dto));

        [TDPermission("UpdateClassAsync", "Sửa lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update")]
        public async Task<ActionResult<Response<ClassRoomDto>>> UpdateClassAsync(ClassRoomDto dto)
            => Ok(await _service.UpdateClassAsync(dto));

        [TDPermission("DeleteAsync", "Xoá lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<ClassRoomDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));

        [TDPermission("AddMemberAsync", "Ghi danh HS vào lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("add-member")]
        public async Task<ActionResult<Response<ClassMemberDto>>> AddMemberAsync(ClassMemberReq req)
            => Ok(await _service.AddMemberAsync(req));

        [TDPermission("UpdateMemberAsync", "Sửa giá riêng HS trong lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("update-member/{idMember:Guid}")]
        public async Task<ActionResult<Response<ClassMemberDto>>> UpdateMemberAsync(Guid idMember, [FromBody] ClassMemberReq req)
            => Ok(await _service.UpdateMemberAsync(idMember, req?.RateOverride));

        [TDPermission("RemoveMemberAsync", "Cho HS rời lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("remove-member/{idMember:Guid}")]
        public async Task<ActionResult<Response<bool>>> RemoveMemberAsync(Guid idMember)
            => Ok(await _service.RemoveMemberAsync(idMember));

        [TDPermission("GenerateScheduleAsync", "Xếp lịch cho lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("generate-schedule")]
        public async Task<ActionResult<Response<int>>> GenerateScheduleAsync(GenerateScheduleReq req)
            => Ok(await _service.GenerateScheduleAsync(req));

        [TDPermission("GetSubjectsAsync", "Danh sách môn của các lớp", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-subjects")]
        public async Task<ActionResult<Response<List<string>>>> GetSubjectsAsync()
            => Ok(await _service.GetSubjectsAsync());

        [TDPermission("GetByStudentAsync", "Lớp của 1 HS", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-student/{idStudent:Guid}")]
        public async Task<ActionResult<Response<List<StudentClassDto>>>> GetByStudentAsync(Guid idStudent)
            => Ok(await _service.GetByStudentAsync(idStudent));
    }
}
