using Microsoft.AspNetCore.Mvc;
using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.Services;
using TD.Lib.Common;
using TD.Lib.Helper;

/*
 * Note*:
 * - Đặt attr TDModule để đánh dấu tạo ra nhóm module
 * - Đặt attr TDPermission để đánh dấu sinh ra mã quyền, những role được phép truy cập
 * - Đặt attr TDAuthorize để thực hiện việc authen & author 
 */
namespace EduTrack.API.Controllers
{
    [TDModule("Quản lý tài khoản", 2)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ApiController
    {
        private static List<Dropdown> LstItem = new List<Dropdown>();
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;

            for (int i = 0; i < 1000; i++)
            {
                LstItem.Add(new Dropdown { Value = i + 1, Text = $"Đào Lê{i + 1}" });
            }
        }

        /// <summary>
        /// Tạo tài khoản
        /// </summary>
        /// <param name="reqDto"></param>
        /// <returns></returns>
        [TDPermission("CreateAsync", "Tạo tài khoản", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("create")]
        [HttpPost]
        public async Task<ActionResult<Response<UserCreateResDto>>> CreateAsync(UserCreateReqDto reqDto)
        {
            return Ok(await _userService.CreateAsync(reqDto));
        }

        /// <summary>
        /// Cập nhập tài khoản
        /// </summary>
        /// <param name="reqDto"></param>
        /// <returns></returns>
        [TDPermission("UpdateAsync", "Cập nhật tài khoản", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("update")]
        [HttpPost]
        public async Task<ActionResult<Response<UserUpdateResDto>>> UpdateAsync(UserUpdateReqDto reqDto)
        {
            return Ok(await _userService.UpdateAsync(reqDto));
        }

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDPermission("DeleteAsync", "Xóa tài khoản", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("delete/{id}")]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> DeleteAsync(Guid id)
        {
            return Ok(await _userService.DeleteAsync(id));
        }

        /// <summary>
        /// Xem chi tiết tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDPermission("GetByIdAsync", "Xem chi tiết tài khoản", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("get-by-id/{id}")]
        [HttpGet]
        public async Task<ActionResult<Response<UserDto>>> GetByIdAsync(Guid id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        /// <summary>
        /// Tìm theo điều kiện
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [TDPermission("GetByFilterAsync", "Xem danh sách tài khoản", $"{RoleCodes.SupperAdmin}, {RoleCodes.Admin}")]
        [TDAuthorize]
        [Route("get-by-filter")]
        [HttpPost]
        public async Task<ActionResult<Response<PagingData<List<UserDto>>>>> GetByFilterAsync(UserGridFilter filter)
        {
            return Ok(await _userService.GetByFilterAsync(filter));
        }

        /// <summary>
        /// Xem chi tiết tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDAuthorize]
        [Route("get-user-profile")]
        [HttpGet]
        public async Task<ActionResult<Response<UserDto>>> GetUserProfileAsync()
        {
            return Ok(await _userService.GetByIdAsync(CurrentUser.Id));
        }

        /// <summary>
        /// Cập nhập thông tin cá nhân
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [TDAuthorize]
        [Route("save-user-profile")]
        [HttpPost]
        public async Task<ActionResult<Response<UserDto>>> SaveUserProfileAsync(UserProfileSaveReq req)
        {
            return Ok(await _userService.SaveUserProfileAsync(req));
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [TDAuthorize]
        [Route("change-password")]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> ChangePasswordAsync(UserChangePasswordReq req)
        {
            return Ok(await _userService.ChangePasswordAsync(req, CurrentUser));
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại
        /// </summary>
        /// <returns></returns>
        [TDAuthorize]
        [Route("get-current-user")]
        [HttpGet]
        public async Task<ActionResult<Response<CurrentUser>>> GetCurrentUserAsync()
        {
            if (CurrentUser is null) return Ok(Response<CurrentUser>.Error(TD.Lib.Common.StatusCode.NotFound, TD.Lib.Common.StatusCode.Ok.ToDescription(), CurrentUser));
            else return Ok(Response<CurrentUser>.Success(CurrentUser, TD.Lib.Common.StatusCode.Ok.ToDescription()));
        }
    }
}
