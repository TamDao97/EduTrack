/*
 * created by:tamdc
 * create date: 07/9/2024
 */

using Microsoft.AspNetCore.Mvc;
using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services;
using TD.Lib.AutoMapper;
using TD.Lib.Common;

namespace EduTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Quản lý quyền", 1)]
    public class RoleController : ApiController
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IRoleService _roleService;

        public RoleController(ILogger<RoleController> logger, IRoleService roleService)
        {
            _logger = logger;
            _roleService = roleService;
        }

        /// <summary>
        /// Lấy tất cả quyền
        /// </summary>
        /// <returns></returns>
        [TDPermission("GetAllAsync", "Lấy tất cả quyền", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("get-all")]
        [HttpGet]
        public virtual async Task<ActionResult<Response<List<RoleDto>>>> GetAllAsync()
        {
            return Ok(await _roleService.GetAllAsync());
        }

        /// <summary>
        /// Xem danh sách 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [TDPermission("GetByFilterAsync", "Xem danh sách quyền", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("get-by-filter")]
        [HttpPost]
        public async Task<ActionResult<Response<PagingData<List<RoleDto>>>>> GetByFilterAsync(RoleGridFilter filter)
        {
            return Ok(await _roleService.GetByFilterAsync(filter));
        }

        /// <summary>
        /// Quét module chức năng
        /// </summary>
        /// <returns></returns>
        [TDPermission("ScanPermissionAsync", "Quét module chức năng", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("scan-permission")]
        [HttpPost]
        public async Task<ActionResult<Response>> ScanPermissionAsync()
        {
            return Ok(await _roleService.ScanPermissionAsync());
        }

        ///// <summary>
        ///// Lấy danh sách module và chức năng
        ///// </summary>
        ///// <returns></returns>
        //[Route("get-permission-by-module")]
        //[HttpGet]
        //public async Task<ActionResult<Response<List<PermissionGroupByModuleDto>>>> GetPermissionGroupByModuleAsync()
        //{
        //    return Ok(await _roleService.GetPermissionGroupByModuleAsync());
        //}

        /// <summary>
        /// Xem module chức năng theo quyền
        /// </summary>
        /// <param name="idRole"></param>
        /// <returns></returns>
        [TDPermission("GetPermissionByRoleAsync", "Xem module chức năng theo quyền", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("get-permission-by-role/{idRole}")]
        [HttpGet]
        public async Task<ActionResult<Response<List<PermissionGroupByModuleDto>>>> GetPermissionByRoleAsync(Guid idRole)
        {
            return Ok(await _roleService.GetPermissionByRoleAsync(idRole));
        }

        /// <summary>
        /// Thêm module chức năng cho quyền
        /// </summary>
        /// <param name="dtoReq"></param>
        /// <returns></returns>
        [TDPermission("AddPermissionByRoleAsync", "Thêm module chức năng cho quyền", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("add-permission-by-role")]
        [HttpPost]
        public async Task<ActionResult<Response<int>>> AddPermissionByRoleAsync(PermissionGroupByRoleCreateReqDto dtoReq)
        {
            return Ok(await _roleService.AddPermissionByRoleAsync(dtoReq));
        }

        /// <summary>
        /// Xem chi tiết
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDPermission("GetByIdAsync", "Xem chi tiết", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("get-by-id/{id:Guid}")]
        [HttpGet]
        public async Task<ActionResult<Response<RoleDto>>> GetByIdAsync(Guid id)
        {
            return Ok(await _roleService.GetByIdAsync(id));
        }

        /// <summary>
        /// Thêm mới
        /// </summary>
        /// <param name="dtoReq"></param>
        /// <returns></returns>
        [TDPermission("CreateAsync", "Thêm mới", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("create")]
        [HttpPost]
        public async Task<ActionResult<Response<RoleDto>>> CreateAsync(RoleDto dtoReq)
        {
            Role entity = AutoMapperGeneric.Map<RoleDto, Role>(dtoReq);
            return Ok(await _roleService.CreateAsync(entity));
        }

        /// <summary>
        /// Cập nhật thông tin
        /// </summary>
        /// <param name="dtoReq"></param>
        /// <returns></returns>
        [TDPermission("UpdateAsync", "Cập nhật thông tin", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("update")]
        [HttpPost]
        public async Task<ActionResult<Response<RoleDto>>> UpdateAsync(RoleDto dtoReq)
        {
            Role entity = AutoMapperGeneric.Map<RoleDto, Role>(dtoReq);
            return Ok(await _roleService.UpdateAsync(entity));
        }

        /// <summary>
        /// Xóa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDPermission("DeleteAsync", "Xóa", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("delete/{id:Guid}")]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> DeleteAsync(Guid id)
        {
            return Ok(await _roleService.DeleteAsync(id));
        }

        /// <summary>
        /// Xóa nhiều
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [TDPermission("DeleteManyAsync", "Xóa nhiều", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("delete-many")]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> DeleteManyAsync(List<Guid> ids)
        {
            return Ok(await _roleService.DeleteManyAsync(ids));
        }
    }
}