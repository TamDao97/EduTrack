using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IParentService : IBaseService<Parent, ParentDto>
    {
        Task<Response<PagingData<List<ParentDto>>>> GetByFilterAsync(ParentGridFilter filter);
    }

    public class ParentService : TutorScopedBaseService<Parent, ParentDto>, IParentService
    {
        public ParentService(IUnitOfWork unitOfWork, IUserContextService userContext)
            : base(unitOfWork, userContext) { }

        public async Task<Response<PagingData<List<ParentDto>>>> GetByFilterAsync(ParentGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = _repos.TableNoTracking
                .Where(r => r.IdTutor == idTutor)
                .OrderByDescending(r => r.DateModify)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                query = query.Where(r => r.FullName.ToLower().Contains(kw) || r.Phone.Contains(kw));
            }

            int total = query.Count();
            var items = query.Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .Select(r => AutoMapperGeneric.Map<Parent, ParentDto>(r))
                             .ToList();

            var paging = PagingData<List<ParentDto>>.Create(items, filter.PageNumber, (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<ParentDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public override async Task<Response<ParentDto>> UpdateAsync(Parent entity)
        {
            entity.MarkDirty(nameof(entity.FullName));
            entity.MarkDirty(nameof(entity.Phone));
            entity.MarkDirty(nameof(entity.Email));
            entity.MarkDirty(nameof(entity.Notes));
            return await base.UpdateAsync(entity);
        }
    }
}
