using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IStudentService : IBaseService<Student, StudentDto>
    {
        Task<Response<PagingData<List<StudentDetailDto>>>> GetByFilterAsync(StudentGridFilter filter);
        Task<Response<StudentDetailDto>> GetDetailByIdAsync(Guid id);
    }

    public class StudentService : TutorScopedBaseService<Student, StudentDto>, IStudentService
    {
        private readonly ITDRepository<Parent> _parentRepos;
        private readonly ISubscriptionService _subService;

        public StudentService(IUnitOfWork unitOfWork, IUserContextService userContext, ISubscriptionService subService)
            : base(unitOfWork, userContext)
        {
            _parentRepos = unitOfWork.GetRepository<Parent>();
            _subService = subService;
        }

        public override async Task<Response<StudentDto>> CreateAsync(Student entity)
        {
            var idTutor = await GetCurrentTutorIdAsync();

            // 1) Quota + active subscription
            var currentCount = await _repos.TableNoTracking.CountAsync(s => s.IdTutor == idTutor);
            var quotaErr = await _subService.CheckCanAddStudentAsync(idTutor, currentCount);
            if (quotaErr != null) return Response<StudentDto>.Error(StatusCode.Forbidden, quotaErr);

            // 2) Parent phải thuộc tutor này
            var parentOk = await _parentRepos.TableNoTracking
                .AnyAsync(p => p.Id == entity.IdParent && p.IdTutor == idTutor);
            if (!parentOk)
                return Response<StudentDto>.Error(StatusCode.BadRequest, "Phụ huynh không hợp lệ");

            return await base.CreateAsync(entity);
        }

        public override async Task<Response<StudentDto>> UpdateAsync(Student entity)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var parentOk = await _parentRepos.TableNoTracking
                .AnyAsync(p => p.Id == entity.IdParent && p.IdTutor == idTutor);
            if (!parentOk)
                return Response<StudentDto>.Error(StatusCode.BadRequest, "Phụ huynh không hợp lệ");

            entity.MarkDirty(nameof(entity.IdParent));
            entity.MarkDirty(nameof(entity.FullName));
            entity.MarkDirty(nameof(entity.DateBirth));
            entity.MarkDirty(nameof(entity.Grade));
            entity.MarkDirty(nameof(entity.Subject));
            entity.MarkDirty(nameof(entity.PerLessonRate));
            entity.MarkDirty(nameof(entity.AvatarFileId));
            entity.MarkDirty(nameof(entity.Status));
            entity.MarkDirty(nameof(entity.StartedAt));
            entity.MarkDirty(nameof(entity.Notes));
            return await base.UpdateAsync(entity);
        }

        public async Task<Response<PagingData<List<StudentDetailDto>>>> GetByFilterAsync(StudentGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = from s in _repos.TableNoTracking
                        join p in _parentRepos.TableNoTracking on s.IdParent equals p.Id into pj
                        from p in pj.DefaultIfEmpty()
                        where s.IdTutor == idTutor
                        select new { s, p };

            if (filter.Status.HasValue)
                query = query.Where(x => x.s.Status == filter.Status.Value);

            if (filter.IdParent.HasValue)
                query = query.Where(x => x.s.IdParent == filter.IdParent.Value);

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                query = query.Where(x => x.s.FullName.ToLower().Contains(kw)
                                       || (x.p != null && x.p.FullName.ToLower().Contains(kw))
                                       || (x.p != null && x.p.Phone.Contains(kw)));
            }

            int total = query.Count();
            var items = query.OrderByDescending(x => x.s.DateModify)
                             .Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .Select(x => new StudentDetailDto
                             {
                                 Id = x.s.Id,
                                 IdTutor = x.s.IdTutor,
                                 IdParent = x.s.IdParent,
                                 FullName = x.s.FullName,
                                 DateBirth = x.s.DateBirth,
                                 Grade = x.s.Grade,
                                 Subject = x.s.Subject,
                                 PerLessonRate = x.s.PerLessonRate,
                                 AvatarFileId = x.s.AvatarFileId,
                                 Status = x.s.Status,
                                 StartedAt = x.s.StartedAt,
                                 Notes = x.s.Notes,
                                 DateCreated = x.s.DateCreated,
                                 DateModify = x.s.DateModify,
                                 ParentFullName = x.p != null ? x.p.FullName : null,
                                 ParentPhone = x.p != null ? x.p.Phone : null,
                                 ParentEmail = x.p != null ? x.p.Email : null,
                             })
                             .ToList();

            var paging = PagingData<List<StudentDetailDto>>.Create(items, filter.PageNumber, (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<StudentDetailDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<StudentDetailDto>> GetDetailByIdAsync(Guid id)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var item = await (from s in _repos.TableNoTracking
                              join p in _parentRepos.TableNoTracking on s.IdParent equals p.Id into pj
                              from p in pj.DefaultIfEmpty()
                              where s.Id == id && s.IdTutor == idTutor
                              select new StudentDetailDto
                              {
                                  Id = s.Id,
                                  IdTutor = s.IdTutor,
                                  IdParent = s.IdParent,
                                  FullName = s.FullName,
                                  DateBirth = s.DateBirth,
                                  Grade = s.Grade,
                                  Subject = s.Subject,
                                  PerLessonRate = s.PerLessonRate,
                                  AvatarFileId = s.AvatarFileId,
                                  Status = s.Status,
                                  StartedAt = s.StartedAt,
                                  Notes = s.Notes,
                                  ParentFullName = p != null ? p.FullName : null,
                                  ParentPhone = p != null ? p.Phone : null,
                                  ParentEmail = p != null ? p.Email : null,
                              }).FirstOrDefaultAsync();

            if (item == null) return Response<StudentDetailDto>.Error(StatusCode.NotFound, "Không tìm thấy");
            return Response<StudentDetailDto>.Success(item, StatusCode.Ok.ToDescription());
        }
    }
}
