using Microsoft.AspNetCore.Mvc;
using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services;
using EduTrack.API.Services.Common;
using TD.Lib.AutoMapper;
using TD.Lib.Common;

namespace EduTrack.API.Controllers
{
    [TDModule("Quản lý menu", 2)]
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ApiController
    {
        private readonly ILogger<PageController> _logger;
        private readonly IPageService _pageService;
        private readonly ICommonService _commonService;

        public PageController(ILogger<PageController> logger, ICommonService commonService, IPageService pageService)
        {
            _logger = logger;
            _pageService = pageService;
            _commonService = commonService;
        }

        /// <summary>
        /// Xem danh sách
        /// </summary>
        /// <returns></returns>
        [TDPermission("GetPageTreeAsync", "Xem danh sách", $"{RoleCodes.SupperAdmin}")]
        [TDAuthorize]
        [Route("get-page-tree")]
        [HttpGet]
        public async Task<ActionResult<Response<List<PageTreeNode>>>> GetPageTreeAsync()
        {
            return Ok(await _pageService.GetPageTreeAsync());
        }

        /// <summary>
        /// Load menu theo user đăng nhập
        /// </summary>
        /// <returns></returns>
        [TDAuthorize]
        [Route("get-page-tree-by-user-login")]
        [HttpGet]
        public async Task<ActionResult<Response<List<PageTreeNode>>>> GetPageTreeByUserLoginAsync()
        {
            return Ok(await _pageService.GetPageTreeByUserLoginAsync());
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
        public async Task<ActionResult<Response<PageDto>>> GetByIdAsync(Guid id)
        {
            return Ok(await _pageService.GetByIdAsync(id));
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
        public async Task<ActionResult<Response<PageDto>>> CreateAsync(PageDto dtoReq)
        {
            Page entity = AutoMapperGeneric.Map<PageDto, Page>(dtoReq);
            return Ok(await _pageService.CreateAsync(entity));
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
        public async Task<ActionResult<Response<PageDto>>> UpdateAsync(PageDto dtoReq)
        {
            Page entity = AutoMapperGeneric.Map<PageDto, Page>(dtoReq);
            return Ok(await _pageService.UpdateAsync(entity));
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
            return Ok(await _pageService.DeleteAsync(id));
        }
    }
}
