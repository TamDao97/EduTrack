using Microsoft.AspNetCore.Mvc;
using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.Services.Base;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.Controllers.Base
{
    public class BaseController<TEntity, TDto> : ApiController where TEntity : BaseEntity, new() where TDto : BaseDto, new()
    {
        private readonly IBaseService<TEntity, TDto> _baseService;

        public BaseController(IBaseService<TEntity, TDto> baseService)
        {
            _baseService = baseService;
        }

        [HttpPost]
        public virtual async Task<ActionResult<Response<TDto>>> CreateAsync(TDto dtoReq)
        {
            TEntity entity = AutoMapperGeneric.Map<TDto, TEntity>(dtoReq);
            return Ok(await _baseService.CreateAsync(entity));
        }

        [HttpPost]
        public virtual async Task<ActionResult<Response<TDto>>> UpdateAsync(TDto dtoReq)
        {
            TEntity entity = AutoMapperGeneric.Map<TDto, TEntity>(dtoReq);
            return Ok(await _baseService.UpdateAsync(entity));
        }

        [HttpPost]
        public virtual async Task<ActionResult<Response<bool>>> DeleteAsync(Guid id)
        {
            return Ok(await _baseService.DeleteAsync(id));
        }

        [HttpPost]
        public virtual async Task<ActionResult<Response<bool>>> DeleteManyAsync(List<Guid> ids)
        {
            return Ok(await _baseService.DeleteManyAsync(ids));
        }

        [HttpGet]
        public virtual async Task<ActionResult<Response<TDto>>> GetByIdAsync(Guid id)
        {
            return Ok(await _baseService.GetByIdAsync(id));
        }

        [HttpGet]
        public virtual async Task<ActionResult<Response<List<TDto>>>> GetAllAsync()
        {
            return Ok(await _baseService.GetAllAsync());
        }
    }
}
