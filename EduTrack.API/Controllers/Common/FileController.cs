using EduTrack.API.Attributes;
using EduTrack.API.Controllers.Base;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.API.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    [TDModule("Quản lý file", 1)]
    public class FileController : ApiController
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [TDAuthorize]
        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] FileUploadRequest file)
        {
            return Ok(await _fileService.UploadFileAsync(file, CurrentUser));
        }

        /// <summary>
        /// Lấy file theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("get-by-id/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _fileService.GetByIdAsync(id));
        }

        /// <summary>
        /// Xóa file theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TDAuthorize]
        [Route("delete/{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _fileService.DeleteAsync(id));
        }
    }
}
