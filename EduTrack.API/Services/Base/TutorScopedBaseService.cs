using EduTrack.API.DataContext.Dto.Base;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.Services.Base
{
    /// <summary>
    /// Base service cho mọi entity nghiệp vụ thuộc gia sư.
    /// Auto-filter `IdTutor = CurrentUser.Id` ở mọi Get/Update/Delete và auto-set khi Create.
    /// Tutor A không thể đọc/sửa/xoá data của tutor B.
    /// </summary>
    public class TutorScopedBaseService<T, TDto> : BaseService<T, TDto>
        where T : BaseEntity, ITutorScoped, new()
        where TDto : BaseDto, new()
    {
        protected readonly IUserContextService _userContext;
        protected readonly ITDRepository<T> _repos;

        public TutorScopedBaseService(IUnitOfWork unitOfWork, IUserContextService userContext) : base(unitOfWork)
        {
            _userContext = userContext;
            _repos = unitOfWork.GetRepository<T>();
        }

        protected async Task<Guid> GetCurrentTutorIdAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            return cu?.Id ?? Guid.Empty;
        }

        public override async Task<Response<TDto>> CreateAsync(T entity)
        {
            entity.IdTutor = await GetCurrentTutorIdAsync();
            return await base.CreateAsync(entity);
        }

        public override async Task<Response<List<TDto>>> GetAllAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var lst = await _repos.TableNoTracking.Where(r => r.IdTutor == idTutor).ToListAsync();
            return Response<List<TDto>>.Success(AutoMapperGeneric.Map<List<T>, List<TDto>>(lst), StatusCode.Ok.ToDescription());
        }

        public override async Task<Response<TDto>> GetByIdAsync(Guid id)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var entity = await _repos.TableNoTracking.FirstOrDefaultAsync(r => r.Id == id && r.IdTutor == idTutor);
            if (entity == null) return Response<TDto>.Error(StatusCode.NotFound, "Không tìm thấy hoặc không thuộc quyền sở hữu của bạn");
            return Response<TDto>.Success(AutoMapperGeneric.Map<T, TDto>(entity), StatusCode.Ok.ToDescription());
        }

        public override async Task<Response<TDto>> UpdateAsync(T entity)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var existing = await _repos.TableNoTracking.AnyAsync(r => r.Id == entity.Id && r.IdTutor == idTutor);
            if (!existing) return Response<TDto>.Error(StatusCode.NotFound, "Không tìm thấy hoặc không thuộc quyền sở hữu của bạn");
            entity.IdTutor = idTutor; // không cho client override
            return await base.UpdateAsync(entity);
        }

        public override async Task<Response<TDto>> DeleteAsync(Guid id, bool isActual = false)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var existing = await _repos.TableNoTracking.AnyAsync(r => r.Id == id && r.IdTutor == idTutor);
            if (!existing) return Response<TDto>.Error(StatusCode.NotFound, "Không tìm thấy hoặc không thuộc quyền sở hữu của bạn");
            return await base.DeleteAsync(id, isActual);
        }
    }
}
