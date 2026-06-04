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
        Task<Response<StudentDto>> CreateWithFirstCourseAsync(StudentDto dto);
    }

    public class StudentService : TutorScopedBaseService<Student, StudentDto>, IStudentService
    {
        private readonly ITDRepository<Parent> _parentRepos;
        private readonly ITDRepository<StudentCourse> _courseRepos;
        private readonly ITDRepository<ClassMember> _memberRepos;
        private readonly ISubscriptionService _subService;

        public StudentService(IUnitOfWork unitOfWork, IUserContextService userContext, ISubscriptionService subService)
            : base(unitOfWork, userContext)
        {
            _parentRepos = unitOfWork.GetRepository<Parent>();
            _courseRepos = unitOfWork.GetRepository<StudentCourse>();
            _memberRepos = unitOfWork.GetRepository<ClassMember>();
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

        /// <summary>
        /// Tạo HS + MÔN HỌC ĐẦU TIÊN trong 1 phát (form Thêm HS): Subject + FirstCourseRate
        /// từ dto seed thành StudentCourse — giá thật từ đây quản lý ở môn/lớp,
        /// Student không còn cột giá riêng.
        /// </summary>
        public async Task<Response<StudentDto>> CreateWithFirstCourseAsync(StudentDto dto)
        {
            var entity = AutoMapperGeneric.Map<StudentDto, Student>(dto);
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

            var rs = await CreateAsync(entity); // quota + parent check + TutorScoped
            if (rs.Status != StatusCode.Ok) return rs;

            await _courseRepos.CreateAsync(new StudentCourse
            {
                Id = Guid.NewGuid(),
                IdTutor = entity.IdTutor,
                IdStudent = entity.Id,
                Subject = string.IsNullOrWhiteSpace(dto.Subject) ? "Chung" : dto.Subject.Trim(),
                PerLessonRate = dto.FirstCourseRate ?? 0,
                IsActive = true,
            });
            await _unitOfWork.SaveChangesAsync();
            return rs;
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

            // Lọc theo LỚP: HS đang ghi danh trong lớp đó
            if (filter.IdClass.HasValue)
                query = query.Where(x => _memberRepos.TableNoTracking
                    .Any(m => m.IdClass == filter.IdClass.Value && m.IdStudent == x.s.Id));

            // Lọc theo MÔN: HS có đăng ký môn đó đang active
            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                var subj = filter.Subject.Trim().ToLower();
                query = query.Where(x => _courseRepos.TableNoTracking
                    .Any(c => c.IdStudent == x.s.Id && c.IsActive && c.Subject.ToLower() == subj));
            }

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

            // Nạp môn ĐANG HỌC của cả trang HS trong 1 query → "Toán · Lý" thay Subject cũ
            var ids = items.Select(i => i.Id!.Value).ToList();
            var courseMap = (await _courseRepos.TableNoTracking
                    .Where(c => ids.Contains(c.IdStudent) && c.IsActive)
                    .Select(c => new { c.IdStudent, c.Subject })
                    .ToListAsync())
                .GroupBy(c => c.IdStudent)
                .ToDictionary(g => g.Key, g => string.Join(" · ", g.Select(x => x.Subject).OrderBy(x => x)));
            foreach (var i in items)
                i.CourseSubjects = courseMap.TryGetValue(i.Id!.Value, out var subj) ? subj : null;

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
                                  AvatarFileId = s.AvatarFileId,
                                  Status = s.Status,
                                  StartedAt = s.StartedAt,
                                  Notes = s.Notes,
                                  ParentFullName = p != null ? p.FullName : null,
                                  ParentPhone = p != null ? p.Phone : null,
                                  ParentEmail = p != null ? p.Email : null,
                              }).FirstOrDefaultAsync();

            if (item == null) return Response<StudentDetailDto>.Error(StatusCode.NotFound, "Không tìm thấy");

            // Môn đang học → "Toán · Lý"
            var subjects = await _courseRepos.TableNoTracking
                .Where(c => c.IdStudent == id && c.IsActive)
                .OrderBy(c => c.Subject)
                .Select(c => c.Subject)
                .ToListAsync();
            item.CourseSubjects = subjects.Count > 0 ? string.Join(" · ", subjects) : null;

            return Response<StudentDetailDto>.Success(item, StatusCode.Ok.ToDescription());
        }
    }
}
