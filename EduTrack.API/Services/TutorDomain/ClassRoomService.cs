using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IClassRoomService : IBaseService<ClassRoom, ClassRoomDto>
    {
        Task<Response<List<ClassRoomDto>>> GetMyClassesAsync();
        Task<Response<PagingData<List<ClassRoomDto>>>> GetByFilterAsync(ClassRoomGridFilter filter);
        Task<Response<ClassRoomDetailDto>> GetDetailAsync(Guid id);
        Task<Response<ClassRoomDto>> CreateClassAsync(ClassRoomDto dto);
        Task<Response<ClassRoomDto>> UpdateClassAsync(ClassRoomDto dto);
        Task<Response<ClassMemberDto>> AddMemberAsync(ClassMemberReq req);
        Task<Response<ClassMemberDto>> UpdateMemberAsync(Guid idMember, decimal? rateOverride);
        Task<Response<bool>> RemoveMemberAsync(Guid idMember);
        Task<Response<int>> GenerateScheduleAsync(GenerateScheduleReq req);
        Task<Response<List<StudentClassDto>>> GetByStudentAsync(Guid idStudent);
        Task<Response<List<string>>> GetSubjectsAsync();
    }

    public class ClassRoomService : TutorScopedBaseService<ClassRoom, ClassRoomDto>, IClassRoomService
    {
        private readonly ITDRepository<ClassMember> _memberRepos;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Lesson> _lessonRepos;
        private readonly INotificationGenerator _notiGen;
        private readonly ISubscriptionService _subService;

        public ClassRoomService(IUnitOfWork uow, IUserContextService ctx,
            INotificationGenerator notiGen, ISubscriptionService subService) : base(uow, ctx)
        {
            _memberRepos = uow.GetRepository<ClassMember>();
            _studentRepos = uow.GetRepository<Student>();
            _lessonRepos = uow.GetRepository<Lesson>();
            _notiGen = notiGen;
            _subService = subService;
        }

        public async Task<Response<List<ClassRoomDto>>> GetMyClassesAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var classes = await _repos.TableNoTracking
                .Where(c => c.IdTutor == idTutor)
                .OrderByDescending(c => c.IsActive).ThenBy(c => c.Name)
                .ToListAsync();

            var ids = classes.Select(c => c.Id).ToList();
            var counts = await _memberRepos.TableNoTracking
                .Where(m => ids.Contains(m.IdClass))
                .GroupBy(m => m.IdClass)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            var dtos = classes.Select(c =>
            {
                var dto = AutoMapperGeneric.Map<ClassRoom, ClassRoomDto>(c);
                dto.MemberCount = counts.TryGetValue(c.Id, out var n) ? n : 0;
                return dto;
            }).ToList();
            return Response<List<ClassRoomDto>>.Success(dtos, StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Danh sách lớp có lọc + paging — dùng cho màn Lớp học (load-more).
        /// Mặc định FE lọc IsActive=true: tutor lâu năm có 200 lớp thì ~190 lớp đã đóng
        /// không đổ ra một lần.
        /// </summary>
        public async Task<Response<PagingData<List<ClassRoomDto>>>> GetByFilterAsync(ClassRoomGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = _repos.TableNoTracking.Where(c => c.IdTutor == idTutor);

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                var subj = filter.Subject.Trim().ToLower();
                query = query.Where(c => c.Subject != null && c.Subject.ToLower() == subj);
            }
            // Lọc theo khoảng KHAI GIẢNG — lớp chưa khai ngày bị loại khi bật lọc thời gian
            if (filter.StartFrom.HasValue)
                query = query.Where(c => c.StartDate != null && c.StartDate >= filter.StartFrom.Value.Date);
            if (filter.StartTo.HasValue)
                query = query.Where(c => c.StartDate != null && c.StartDate <= filter.StartTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(kw)
                                      || (c.Subject != null && c.Subject.ToLower().Contains(kw)));
            }

            int total = await query.CountAsync();
            var classes = await query
                .OrderByDescending(c => c.IsActive).ThenBy(c => c.Name)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // MemberCount cho riêng trang này — 1 query
            var ids = classes.Select(c => c.Id).ToList();
            var counts = await _memberRepos.TableNoTracking
                .Where(m => ids.Contains(m.IdClass))
                .GroupBy(m => m.IdClass)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            var dtos = classes.Select(c =>
            {
                var dto = AutoMapperGeneric.Map<ClassRoom, ClassRoomDto>(c);
                dto.MemberCount = counts.TryGetValue(c.Id, out var n) ? n : 0;
                return dto;
            }).ToList();

            var paging = PagingData<List<ClassRoomDto>>.Create(dtos, filter.PageNumber,
                (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<ClassRoomDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<ClassRoomDetailDto>> GetDetailAsync(Guid id)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var cls = await _repos.TableNoTracking.FirstOrDefaultAsync(c => c.Id == id && c.IdTutor == idTutor);
            if (cls == null) return Response<ClassRoomDetailDto>.Error(StatusCode.NotFound, "Không tìm thấy lớp");

            var members = await (from m in _memberRepos.TableNoTracking
                                 join s in _studentRepos.TableNoTracking on m.IdStudent equals s.Id
                                 where m.IdClass == id
                                 orderby s.FullName
                                 select new ClassMemberDto
                                 {
                                     Id = m.Id,
                                     IdClass = m.IdClass,
                                     IdStudent = m.IdStudent,
                                     StudentFullName = s.FullName,
                                     RateOverride = m.RateOverride,
                                     EffectiveRate = m.RateOverride ?? cls.DefaultRatePerLesson,
                                 }).ToListAsync();

            var dto = AutoMapperGeneric.Map<ClassRoom, ClassRoomDetailDto>(cls);
            dto.MemberCount = members.Count;
            dto.Members = members;
            return Response<ClassRoomDetailDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<ClassRoomDto>> CreateClassAsync(ClassRoomDto dto)
        {
            var err = Validate(dto);
            if (err != null) return Response<ClassRoomDto>.Error(StatusCode.BadRequest, err);

            var idTutor = await GetCurrentTutorIdAsync();
            var entity = new ClassRoom
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                Name = dto.Name.Trim(),
                Subject = dto.Subject?.Trim(),
                DefaultRatePerLesson = dto.DefaultRatePerLesson,
                DaysOfWeek = NormalizeDays(dto.DaysOfWeek),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location,
                StartDate = dto.StartDate?.Date,
                EndDate = dto.EndDate?.Date,
                IsActive = dto.IsActive,
                Notes = dto.Notes,
            };
            await _repos.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<ClassRoomDto>.Success(
                AutoMapperGeneric.Map<ClassRoom, ClassRoomDto>(entity), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<ClassRoomDto>> UpdateClassAsync(ClassRoomDto dto)
        {
            var err = Validate(dto);
            if (err != null) return Response<ClassRoomDto>.Error(StatusCode.BadRequest, err);

            var idTutor = await GetCurrentTutorIdAsync();
            var entity = await _repos.Table.FirstOrDefaultAsync(c => c.Id == dto.Id && c.IdTutor == idTutor);
            if (entity == null) return Response<ClassRoomDto>.Error(StatusCode.NotFound, "Không tìm thấy lớp");

            entity.Name = dto.Name.Trim();
            entity.Subject = dto.Subject?.Trim();
            entity.DefaultRatePerLesson = dto.DefaultRatePerLesson;
            entity.DaysOfWeek = NormalizeDays(dto.DaysOfWeek);
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.Location = dto.Location;
            entity.StartDate = dto.StartDate?.Date;
            entity.EndDate = dto.EndDate?.Date;
            entity.IsActive = dto.IsActive;
            entity.Notes = dto.Notes;
            entity.MarkDirty(nameof(entity.StartDate));
            entity.MarkDirty(nameof(entity.EndDate));
            entity.MarkDirty(nameof(entity.Name));
            entity.MarkDirty(nameof(entity.Subject));
            entity.MarkDirty(nameof(entity.DefaultRatePerLesson));
            entity.MarkDirty(nameof(entity.DaysOfWeek));
            entity.MarkDirty(nameof(entity.StartTime));
            entity.MarkDirty(nameof(entity.EndTime));
            entity.MarkDirty(nameof(entity.Location));
            entity.MarkDirty(nameof(entity.IsActive));
            entity.MarkDirty(nameof(entity.Notes));
            await _unitOfWork.SaveChangesAsync();

            return Response<ClassRoomDto>.Success(
                AutoMapperGeneric.Map<ClassRoom, ClassRoomDto>(entity), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<ClassMemberDto>> AddMemberAsync(ClassMemberReq req)
        {
            var idTutor = await GetCurrentTutorIdAsync();

            var cls = await _repos.TableNoTracking.FirstOrDefaultAsync(c => c.Id == req.IdClass && c.IdTutor == idTutor);
            if (cls == null) return Response<ClassMemberDto>.Error(StatusCode.NotFound, "Không tìm thấy lớp");

            var student = await _studentRepos.TableNoTracking
                .FirstOrDefaultAsync(s => s.Id == req.IdStudent && s.IdTutor == idTutor);
            if (student == null) return Response<ClassMemberDto>.Error(StatusCode.BadRequest, "Học sinh không hợp lệ");

            var existed = await _memberRepos.TableNoTracking
                .AnyAsync(m => m.IdClass == req.IdClass && m.IdStudent == req.IdStudent);
            if (existed) return Response<ClassMemberDto>.Error(StatusCode.BadRequest, $"{student.FullName} đã ở trong lớp này");

            var member = new ClassMember
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                IdClass = req.IdClass,
                IdStudent = req.IdStudent,
                RateOverride = req.RateOverride,
            };
            await _memberRepos.CreateAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return Response<ClassMemberDto>.Success(new ClassMemberDto
            {
                Id = member.Id,
                IdClass = member.IdClass,
                IdStudent = member.IdStudent,
                StudentFullName = student.FullName,
                RateOverride = member.RateOverride,
                EffectiveRate = member.RateOverride ?? cls.DefaultRatePerLesson,
            }, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<ClassMemberDto>> UpdateMemberAsync(Guid idMember, decimal? rateOverride)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var member = await _memberRepos.Table.FirstOrDefaultAsync(m => m.Id == idMember && m.IdTutor == idTutor);
            if (member == null) return Response<ClassMemberDto>.Error(StatusCode.NotFound, "Không tìm thấy ghi danh");

            member.RateOverride = rateOverride;
            member.MarkDirty(nameof(member.RateOverride));
            await _unitOfWork.SaveChangesAsync();
            return Response<ClassMemberDto>.Success(
                AutoMapperGeneric.Map<ClassMember, ClassMemberDto>(member), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<bool>> RemoveMemberAsync(Guid idMember)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var member = await _memberRepos.Table.FirstOrDefaultAsync(m => m.Id == idMember && m.IdTutor == idTutor);
            if (member == null) return Response<bool>.Error(StatusCode.NotFound, "Không tìm thấy ghi danh");

            member.IsDeleted = true;
            member.DateDeleted = AppTime.VnNow;
            member.MarkDirty(nameof(member.IsDeleted));
            member.MarkDirty(nameof(member.DateDeleted));
            await _unitOfWork.SaveChangesAsync();
            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Xếp lịch N tuần: với mỗi ngày khớp lịch lớp trong [StartDate, StartDate+N*7),
        /// sinh 1 Lesson/HS (cùng GroupKey theo ca). Idempotent: ngày nào HS đã có buổi
        /// của lớp này thì bỏ qua (chạy lại không tạo trùng).
        /// </summary>
        public async Task<Response<int>> GenerateScheduleAsync(GenerateScheduleReq req)
        {
            var idTutor = await GetCurrentTutorIdAsync();

            var subErr = await _subService.CheckCanWriteAsync(idTutor);
            if (subErr != null) return Response<int>.Error(StatusCode.Forbidden, subErr);

            if (req.NumberOfWeeks < 1 || req.NumberOfWeeks > 52)
                return Response<int>.Error(StatusCode.BadRequest, "Số tuần phải từ 1 đến 52");

            var cls = await _repos.TableNoTracking.FirstOrDefaultAsync(c => c.Id == req.IdClass && c.IdTutor == idTutor);
            if (cls == null) return Response<int>.Error(StatusCode.NotFound, "Không tìm thấy lớp");
            if (!cls.IsActive) return Response<int>.Error(StatusCode.BadRequest, "Lớp đã đóng — mở lại trước khi xếp lịch");

            var days = ParseDays(cls.DaysOfWeek);
            if (days.Count == 0) return Response<int>.Error(StatusCode.BadRequest, "Lớp chưa khai lịch thứ trong tuần");

            var members = await _memberRepos.TableNoTracking
                .Where(m => m.IdClass == cls.Id)
                .Select(m => new { m.IdStudent, m.RateOverride })
                .ToListAsync();
            if (members.Count == 0) return Response<int>.Error(StatusCode.BadRequest, "Lớp chưa có học sinh — ghi danh trước");

            var from = req.StartDate.Date;
            var to = from.AddDays(7 * req.NumberOfWeeks);

            // Cắt theo thời gian MỞ LỚP: không sinh buổi trước khai giảng / sau ngày kết thúc
            if (cls.StartDate.HasValue && from < cls.StartDate.Value.Date) from = cls.StartDate.Value.Date;
            if (cls.EndDate.HasValue && to > cls.EndDate.Value.Date.AddDays(1)) to = cls.EndDate.Value.Date.AddDays(1);
            if (from >= to)
                return Response<int>.Error(StatusCode.BadRequest,
                    "Khoảng xếp lịch nằm ngoài thời gian mở lớp (khai giảng → kết thúc)");

            // Buổi đã tồn tại của lớp trong khoảng → idempotent
            var existing = await _lessonRepos.TableNoTracking
                .Where(l => l.IdClass == cls.Id && l.ScheduledDate >= from && l.ScheduledDate < to)
                .Select(l => new { l.IdStudent, l.ScheduledDate })
                .ToListAsync();
            var existingSet = existing.Select(e => (e.IdStudent, e.ScheduledDate.Date)).ToHashSet();

            var lessons = new List<Lesson>();
            for (var date = from; date < to; date = date.AddDays(1))
            {
                if (!days.Contains(date.DayOfWeek)) continue;
                var groupKey = Guid.NewGuid(); // 1 ca = 1 GroupKey
                foreach (var m in members)
                {
                    if (existingSet.Contains((m.IdStudent, date))) continue;
                    lessons.Add(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        IdTutor = idTutor,
                        IdStudent = m.IdStudent,
                        IdClass = cls.Id,
                        GroupKey = groupKey,
                        ScheduledDate = date,
                        StartTime = cls.StartTime,
                        EndTime = cls.EndTime,
                        Location = cls.Location,
                        Status = LessonStatusEnums.Scheduled,
                        ChargeAmount = m.RateOverride ?? cls.DefaultRatePerLesson,
                    });
                }
            }

            if (lessons.Count == 0)
                return Response<int>.Success(0, "Không có buổi mới (lịch khoảng này đã xếp đủ)");

            await _lessonRepos.CreateMultiAsync(lessons);
            await _unitOfWork.SaveChangesAsync();

            foreach (var l in lessons)
                await _notiGen.GenerateForLessonAsync(l);

            return Response<int>.Success(lessons.Count, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<StudentClassDto>>> GetByStudentAsync(Guid idStudent)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var list = await (from m in _memberRepos.TableNoTracking
                              join c in _repos.TableNoTracking on m.IdClass equals c.Id
                              where m.IdStudent == idStudent && m.IdTutor == idTutor && c.IsActive
                              orderby c.Name
                              select new { c, m.RateOverride }).ToListAsync();

            var dtos = list.Select(x => new StudentClassDto
            {
                IdClass = x.c.Id,
                ClassName = x.c.Name,
                Subject = x.c.Subject,
                EffectiveRate = x.RateOverride ?? x.c.DefaultRatePerLesson,
                ScheduleLabel = BuildScheduleLabel(x.c),
            }).ToList();
            return Response<List<StudentClassDto>>.Success(dtos, StatusCode.Ok.ToDescription());
        }

        /// <summary>Danh sách MÔN distinct của các lớp tutor — cho dropdown lọc.</summary>
        public async Task<Response<List<string>>> GetSubjectsAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var subjects = await _repos.TableNoTracking
                .Where(c => c.IdTutor == idTutor && c.Subject != null && c.Subject != "")
                .Select(c => c.Subject!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            return Response<List<string>>.Success(subjects, StatusCode.Ok.ToDescription());
        }

        #region helpers
        private static string? Validate(ClassRoomDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return "Vui lòng nhập tên lớp";
            if (dto.DefaultRatePerLesson < 0) return "Giá buổi không hợp lệ";
            if (ParseDays(dto.DaysOfWeek).Count == 0) return "Chọn ít nhất 1 thứ trong tuần";
            if (dto.EndTime <= dto.StartTime) return "Giờ kết thúc phải sau giờ bắt đầu";
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Value.Date)
                return "Ngày kết thúc phải sau ngày khai giảng";
            return null;
        }

        private static List<DayOfWeek> ParseDays(string? csv)
            => (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) && n >= 0 && n <= 6 ? (DayOfWeek?)(DayOfWeek)n : null)
                .Where(d => d.HasValue).Select(d => d!.Value).Distinct().ToList();

        private static string NormalizeDays(string? csv)
            => string.Join(",", ParseDays(csv).OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d).Select(d => (int)d));

        private static string BuildScheduleLabel(ClassRoom c)
        {
            var names = ParseDays(c.DaysOfWeek)
                .OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d)
                .Select(d => d == DayOfWeek.Sunday ? "CN" : $"T{(int)d + 1}");
            return $"{string.Join("·", names)} {c.StartTime:hh\\:mm}-{c.EndTime:hh\\:mm}";
        }
        #endregion
    }
}
