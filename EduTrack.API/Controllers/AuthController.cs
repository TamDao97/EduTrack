using Microsoft.AspNetCore.Mvc;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.Services;
using TD.Lib.Common;

namespace EduTrack.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        /// <summary>Đăng ký user mới — KHÔNG gán role (legacy)</summary>
        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> RegisterAsync(RegisterReq req)
        {
            return Ok(await _authService.RegisterAsync(req));
        }

        /// <summary>Đăng nhập</summary>
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<Response<CurrentUser>>> LoginAsync(LoginReq req)
        {
            return Ok(await _authService.LoginAsync(req));
        }

        /// <summary>Self-signup cho gia sư — tạo user mới + gán Role TUTOR + trả JWT (auto-login).</summary>
        [Route("signup-tutor")]
        [HttpPost]
        public async Task<ActionResult<Response<CurrentUser>>> SignupTutorAsync(SignupTutorReq req)
        {
            return Ok(await _authService.SignupTutorAsync(req));
        }
    }
}
