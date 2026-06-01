using EduTrack.API.Attributes;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.Base;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Controllers.TutorDomain
{
    /// <summary>
    /// Tutor xem subscription của chính họ + lịch sử thanh toán → trang /billing.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Gói thanh toán", 45)]
    public class SubscriptionController : ApiController
    {
        private readonly ISubscriptionService _service;
        private readonly IUserContextService _userContext;

        public SubscriptionController(ISubscriptionService service, IUserContextService userContext)
        {
            _service = service;
            _userContext = userContext;
        }

        [TDAuthorize]
        [HttpGet("get-mine")]
        public async Task<IActionResult> GetMineAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Unauthorized();
            var sub = await _service.GetMineAsync(cu.Id);
            return Ok(Response<MySubscriptionDto>.Success(sub!, TD.Lib.Common.StatusCode.Ok.ToDescription()));
        }

        [TDAuthorize]
        [HttpGet("get-my-payments")]
        public async Task<IActionResult> GetMyPaymentsAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Unauthorized();
            var list = await _service.GetMyPaymentsAsync(cu.Id);
            return Ok(Response<List<MyPaymentDto>>.Success(list, TD.Lib.Common.StatusCode.Ok.ToDescription()));
        }
    }
}
