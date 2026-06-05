using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.Services.TutorDomain;
using Microsoft.AspNetCore.Mvc;
using TD.Lib.Common;

namespace EduTrack.API.Controllers.TutorDomain
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Kỳ học phí", 40)]
    public class TuitionPeriodController : ApiController
    {
        private readonly ITuitionPeriodService _service;
        public TuitionPeriodController(ITuitionPeriodService service) { _service = service; }

        [TDPermission("GetByFilterAsync", "Danh sách kỳ học phí", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("get-by-filter")]
        public async Task<ActionResult<Response<PagingData<List<TuitionPeriodDetailDto>>>>> GetByFilterAsync(TuitionPeriodGridFilter filter)
            => Ok(await _service.GetByFilterAsync(filter));

        [TDPermission("GetByIdAsync", "Xem kỳ học phí", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-by-id/{id:Guid}")]
        public async Task<ActionResult<Response<TuitionPeriodDto>>> GetByIdAsync(Guid id)
            => Ok(await _service.GetByIdAsync(id));

        [TDPermission("OpenOrGetAsync", "Mở kỳ học phí", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("open-or-get")]
        public async Task<ActionResult<Response<TuitionPeriodDto>>> OpenOrGetAsync([FromBody] OpenOrGetReq req)
            => Ok(await _service.OpenOrGetAsync(req.IdStudent, req.Month, req.Year));

        [TDPermission("PreviewAsync", "Xem trước số tiền chốt kỳ", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("preview")]
        public async Task<ActionResult<Response<TuitionPreviewDto>>> PreviewAsync(
            [FromQuery] Guid idStudent, [FromQuery] int month, [FromQuery] int year)
            => Ok(await _service.PreviewAsync(idStudent, month, year));

        [TDPermission("CloseAsync", "Chốt kỳ học phí", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("close/{id:Guid}")]
        public async Task<ActionResult<Response<TuitionPeriodDto>>> CloseAsync(Guid id, [FromBody] CloseReq req)
            => Ok(await _service.CloseAsync(id, req?.Adjustment ?? 0, req?.Notes));

        [TDPermission("PreviewMonthAsync", "Bảng chốt kỳ tháng", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("preview-month")]
        public async Task<ActionResult<Response<MonthClosePreviewDto>>> PreviewMonthAsync([FromQuery] int month, [FromQuery] int year)
            => Ok(await _service.PreviewMonthAsync(month, year));

        [TDPermission("CloseMonthBulkAsync", "Chốt kỳ hàng loạt", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("close-month-bulk")]
        public async Task<ActionResult<Response<MonthCloseResultDto>>> CloseMonthBulkAsync(MonthCloseBulkReq req)
            => Ok(await _service.CloseMonthBulkAsync(req));

        [TDPermission("RecordPaymentAsync", "Ghi nhận thanh toán", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("record-payment/{id:Guid}")]
        public async Task<ActionResult<Response<TuitionPeriodDto>>> RecordPaymentAsync(Guid id, [FromBody] PaymentReq req)
            => Ok(await _service.RecordPaymentAsync(id, req.Amount, req.Notes, req.Method));

        [TDPermission("GetPaymentsAsync", "Lịch sử thu của kỳ", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpGet("get-payments/{idPeriod:Guid}")]
        public async Task<ActionResult<Response<List<TuitionPaymentDto>>>> GetPaymentsAsync(Guid idPeriod)
            => Ok(await _service.GetPaymentsAsync(idPeriod));

        [TDPermission("DeleteAsync", "Xoá kỳ học phí", $"{RoleCodes.Tutor}")]
        [TDAuthorize]
        [HttpPost("delete/{id:Guid}")]
        public async Task<ActionResult<Response<TuitionPeriodDto>>> DeleteAsync(Guid id)
            => Ok(await _service.DeleteAsync(id));

        public class OpenOrGetReq { public Guid IdStudent { get; set; } public int Month { get; set; } public int Year { get; set; } }
        public class CloseReq { public decimal Adjustment { get; set; } public string? Notes { get; set; } }
        public class PaymentReq { public decimal Amount { get; set; } public string? Notes { get; set; } public string? Method { get; set; } }
    }
}
