using Microsoft.EntityFrameworkCore;
using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.UnitOfWork;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.Services.Base
{
    public interface IBaseService<T, TDto> where T : BaseEntity, new() where TDto : BaseDto, new()
    {
        #region CRUD
        Task<Response<TDto>> CreateAsync(T entity);
        Task<Response<TDto>> UpdateAsync(T entity);
        Task<Response<TDto>> DeleteAsync(Guid id, bool isActual = false);
        Task<Response<int>> DeleteManyAsync(List<Guid> ids, bool isActual = false);
        #endregion

        #region GET
        Task<Response<List<TDto>>> GetAllAsync();
        Task<Response<TDto>> GetByIdAsync(Guid id);
        #endregion

        #region Validation logic
        public bool IsDuplicated(ref string errorMess, string fieldCheck, object valueCheck, object idValue = null);
        #endregion
    }

    public class BaseService<T, TDto> : IBaseService<T, TDto> where T : BaseEntity, new() where TDto : BaseDto, new()
    {
        protected readonly IUnitOfWork _unitOfWork;

        public BaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public virtual async Task<Response<TDto>> CreateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException();
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

            entity = await _unitOfWork.GetRepository<T>().CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<TDto>.Success(AutoMapperGeneric.Map<T, TDto>(entity), StatusCode.Ok.ToDescription());
        }

        public virtual async Task<Response<TDto>> UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException();

            entity = await _unitOfWork.GetRepository<T>().UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<TDto>.Success(AutoMapperGeneric.Map<T, TDto>(entity), StatusCode.Ok.ToDescription());
        }

        public virtual async Task<Response<TDto>> DeleteAsync(Guid id, bool isActual = false)
        {
            T entity = await _unitOfWork.GetRepository<T>().GetByIdAsync(id);

            if (entity == null) return Response<TDto>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());

            _unitOfWork.Context.IsHardDeleteMode = isActual;
            entity = await _unitOfWork.GetRepository<T>().DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<TDto>.Success(AutoMapperGeneric.Map<T, TDto>(entity), StatusCode.Ok.ToDescription());
        }

        public virtual async Task<Response<int>> DeleteManyAsync(List<Guid> ids, bool isActual = false)
        {
            List<T> entities = await _unitOfWork.GetRepository<T>().Table.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (!entities.Any()) return Response<int>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());

            _unitOfWork.Context.IsHardDeleteMode = isActual;
            await _unitOfWork.GetRepository<T>().DeleteMultiAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            return Response<int>.Success(entities.Count, StatusCode.Ok.ToDescription());
        }

        public virtual async Task<Response<List<TDto>>> GetAllAsync()
        {
            List<T> lstEntities = _unitOfWork.GetRepository<T>().TableNoTracking.ToList();
            if (lstEntities == null) return Response<List<TDto>>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            return Response<List<TDto>>.Success(AutoMapperGeneric.Map<List<T>, List<TDto>>(lstEntities), StatusCode.Ok.ToDescription());
        }

        public virtual async Task<Response<TDto>> GetByIdAsync(Guid id)
        {
            T entity = await _unitOfWork.GetRepository<T>().GetByIdAsync(id);
            if (entity == null) return Response<TDto>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            return Response<TDto>.Success(AutoMapperGeneric.Map<T, TDto>(entity), StatusCode.Ok.ToDescription());
        }

        #region Validation logic
        public bool IsDuplicated(ref string errorMess, string fieldCheck, object valueCheck, object idValue = null)
        {
            //Equals func dùng để so sánh giá trị của 2 object
            if (_unitOfWork.GetRepository<T>().TableNoTracking.AsEnumerable().Any(r => !r.GetType().GetProperty("Id").GetValue(r, null).Equals(idValue) && valueCheck.Equals(r.GetType().GetProperty(fieldCheck).GetValue(r, null))))
            {
                errorMess = string.Format(MessageText.Duplicate, fieldCheck);
                return true;
            }
            else return false;
        }
        #endregion
    }
}
