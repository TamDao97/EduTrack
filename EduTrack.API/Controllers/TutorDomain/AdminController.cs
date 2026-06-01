using EduTrack.API.Attributes;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.Base;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    /// <summary>
    /// Founder dashboard — chỉ user có IsSuper=true truy cập.
    /// Để bypass [TDAuthorize] permission check (vì admin chưa được seed Permission codes
    /// cho các action mới), guard bằng IsSuper check trong từng action.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Founder admin", 100)]
    public class AdminController : ApiController
    {
        private readonly IAdminService _service;
        private readonly IUserContextService _userContext;

        public AdminController(IAdminService service, IUserContextService userContext)
        {
            _service = service;
            _userContext = userContext;
        }

        private async Task<(bool ok, Guid adminId, IActionResult? forbid)> AssertSuperAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return (false, Guid.Empty, Unauthorized());
            if (!cu.IsSuper) return (false, Guid.Empty, StatusCode(403, new { message = "Cần quyền Super Admin" }));
            return (true, cu.Id, null);
        }

        [TDAuthorize]
        [HttpPost("get-tutors")]
        public async Task<IActionResult> GetTutorsAsync(AdminTutorFilter filter)
        {
            var (ok, _, forbid) = await AssertSuperAsync();
            if (!ok) return forbid!;
            return Ok(await _service.GetTutorsAsync(filter));
        }

        [TDAuthorize]
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPaymentAsync(ConfirmPaymentReq req)
        {
            var (ok, adminId, forbid) = await AssertSuperAsync();
            if (!ok) return forbid!;
            return Ok(await _service.ConfirmPaymentAsync(req, adminId));
        }

        [TDAuthorize]
        [HttpGet("get-stats")]
        public async Task<IActionResult> GetStatsAsync()
        {
            var (ok, _, forbid) = await AssertSuperAsync();
            if (!ok) return forbid!;
            return Ok(await _service.GetStatsAsync());
        }

        [TDAuthorize]
        [HttpPost("get-payments")]
        public async Task<IActionResult> GetPaymentsAsync(GridFilterBase filter)
        {
            var (ok, _, forbid) = await AssertSuperAsync();
            if (!ok) return forbid!;
            return Ok(await _service.GetPaymentsAsync(filter));
        }
    }
}
