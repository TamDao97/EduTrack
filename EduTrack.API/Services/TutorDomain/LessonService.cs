using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface ILessonService : IBaseService<Lesson, LessonDto>
    {
        Task<Response<PagingData<List<LessonDetailDto>>>> GetByFilterAsync(LessonGridFilter filter);
        Task<Response<List<LessonDetailDto>>> GetWeekAsync(DateTime weekStart);
        Task<Response<int>> BulkCreateRecurringAsync(LessonBulkCreateReq req);
        Task<Response<LessonDto>> MarkDoneAsync(Guid id);
        Task<Response<int>> MarkDonePastAsync();
        Task<Response<LessonDto>> CancelAsync(Guid id, string? reason);
    }

    public class LessonService : TutorScopedBaseService<Lesson, LessonDto>, ILessonService
    {
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Parent> _parentRepos;
        private readonly ITDRepository<StudentCourse> _courseRepos;
        private readonly INotificationGenerator _notiGen;
        private readonly ISubscriptionService _subService;

        public LessonService(IUnitOfWork unitOfWork, IUserContextService userContext, INotificationGenerator notiGen, ISubscriptionService subService)
            : base(unitOfWork, userContext)
        {
            _studentRepos = unitOfWork.GetRepository<Student>();
            _parentRepos = unitOfWork.GetRepository<Parent>();
            _courseRepos = unitOfWork.GetRepository<StudentCourse>();
            _notiGen = notiGen;
            _subService = subService;
        }

        /// <summary>
        /// Resolve giá buổi: có IdCourse → lấy giá CỦA MÔN ĐÓ (validate course thuộc đúng HS);
        /// không có → fallback giá chung của HS (tương thích dữ liệu cũ).
        /// Trả (rate, error).
        /// </summary>
        private async Task<(decimal rate, string? error)> ResolveRateAsync(Guid? idCourse, Student student)
        {
            if (!idCourse.HasValue) return (student.PerLessonRate, null);
            var course = await _courseRepos.TableNoTracking
                .FirstOrDefaultAsync(c => c.Id == idCourse.Value && c.IdStudent == student.Id && c.IdTutor == student.IdTutor);
            if (course == null) return (0, "Môn học không hợp lệ");
            return (course.PerLessonRate, null);
        }

        public override async Task<Response<LessonDto>> CreateAsync(Lesson entity)
        {
            var idTutor = await GetCurrentTutorIdAsync();

            var subErr = await _subService.CheckCanWriteAsync(idTutor);
            if (subErr != null) return Response<LessonDto>.Error(StatusCode.Forbidden, subErr);

            var student = await _studentRepos.TableNoTracking
                .FirstOrDefaultAsync(s => s.Id == entity.IdStudent && s.IdTutor == idTutor);
            if (student == null)
                return Response<LessonDto>.Error(StatusCode.BadRequest, "Học sinh không hợp lệ");

            // Snapshot ChargeAmount: theo MÔN nếu có IdCourse, fallback giá chung của HS
            var (rate, rateErr) = await ResolveRateAsync(entity.IdCourse, student);
            if (rateErr != null) return Response<LessonDto>.Error(StatusCode.BadRequest, rateErr);
            entity.ChargeAmount = rate;
            entity.Status = LessonStatusEnums.Scheduled;
            entity.IdTuitionPeriod = null;
            entity.IdTutor = idTutor;

            var rs = await base.CreateAsync(entity);
            if (rs.Status == StatusCode.Ok)
                await _notiGen.GenerateForLessonAsync(entity);
            return rs;
        }

        public override async Task<Response<LessonDto>> UpdateAsync(Lesson entity)
        {
            // Lesson đã gom vào TuitionPeriod (closed) thì không cho sửa nữa
            var idTutor = await GetCurrentTutorIdAsync();
            var existing = await _repos.TableNoTracking
                .FirstOrDefaultAsync(l => l.Id == entity.Id && l.IdTutor == idTutor);
            if (existing == null)
                return Response<LessonDto>.Error(StatusCode.NotFound, "Không tìm thấy");
            if (existing.IdTuitionPeriod.HasValue)
                return Response<LessonDto>.Error(StatusCode.BadRequest, "Buổi học đã được chốt vào kỳ học phí, không thể sửa");

            entity.MarkDirty(nameof(entity.IdCourse));
            entity.MarkDirty(nameof(entity.ScheduledDate));
            entity.MarkDirty(nameof(entity.StartTime));
            entity.MarkDirty(nameof(entity.EndTime));
            entity.MarkDirty(nameof(entity.Location));
            entity.MarkDirty(nameof(entity.Status));
            entity.MarkDirty(nameof(entity.ChargeAmount));
            entity.MarkDirty(nameof(entity.DoneAt));
            entity.MarkDirty(nameof(entity.Notes));
            return await base.UpdateAsync(entity);
        }

        public async Task<Response<int>> BulkCreateRecurringAsync(LessonBulkCreateReq req)
        {
            var idTutor = await GetCurrentTutorIdAsync();

            var subErr = await _subService.CheckCanWriteAsync(idTutor);
            if (subErr != null) return Response<int>.Error(StatusCode.Forbidden, subErr);

            var student = await _studentRepos.TableNoTracking
                .FirstOrDefaultAsync(s => s.Id == req.IdStudent && s.IdTutor == idTutor);
            if (student == null)
                return Response<int>.Error(StatusCode.BadRequest, "Học sinh không hợp lệ");
            if (req.DaysOfWeek == null || req.DaysOfWeek.Count == 0)
                return Response<int>.Error(StatusCode.BadRequest, "Cần chọn ít nhất 1 ngày trong tuần");
            if (req.NumberOfWeeks <= 0 || req.NumberOfWeeks > 52)
                return Response<int>.Error(StatusCode.BadRequest, "Số tuần phải từ 1 đến 52");

            // Giá theo MÔN nếu chọn, fallback giá chung của HS
            var (snapshotRate, rateErr) = await ResolveRateAsync(req.IdCourse, student);
            if (rateErr != null) return Response<int>.Error(StatusCode.BadRequest, rateErr);

            var lessons = new List<Lesson>();
            for (int w = 0; w < req.NumberOfWeeks; w++)
            {
                foreach (var dow in req.DaysOfWeek.Distinct())
                {
                    var weekStart = req.StartDate.Date.AddDays(7 * w);
                    var dayOffset = ((int)dow - (int)weekStart.DayOfWeek + 7) % 7;
                    var date = weekStart.AddDays(dayOffset);
                    lessons.Add(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        IdTutor = idTutor,
                        IdStudent = req.IdStudent,
                        IdCourse = req.IdCourse,
                        ScheduledDate = date,
                        StartTime = req.StartTime,
                        EndTime = req.EndTime,
                        Location = req.Location,
                        Status = LessonStatusEnums.Scheduled,
                        ChargeAmount = snapshotRate,
                    });
                }
            }

            await _repos.CreateMultiAsync(lessons);
            await _unitOfWork.SaveChangesAsync();

            // Sinh nhắc cho từng buổi đã tạo
            foreach (var l in lessons)
                await _notiGen.GenerateForLessonAsync(l);

            return Response<int>.Success(lessons.Count, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<LessonDto>> MarkDoneAsync(Guid id)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var lesson = await _repos.Table.FirstOrDefaultAsync(l => l.Id == id && l.IdTutor == idTutor);
            if (lesson == null) return Response<LessonDto>.Error(StatusCode.NotFound, "Không tìm thấy");
            if (lesson.IdTuitionPeriod.HasValue)
                return Response<LessonDto>.Error(StatusCode.BadRequest, "Buổi học đã chốt vào kỳ học phí");

            lesson.Status = LessonStatusEnums.Done;
            lesson.DoneAt = AppTime.VnNow;
            lesson.MarkDirty(nameof(lesson.Status));
            lesson.MarkDirty(nameof(lesson.DoneAt));
            await _repos.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            // Buổi đã dạy thì không nhắc nữa
            await _notiGen.CancelForLessonAsync(lesson.Id);

            return Response<LessonDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<Lesson, LessonDto>(lesson), StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Đánh dấu "Đã dạy" HÀNG LOẠT mọi buổi "Đã lên lịch" đã qua giờ kết thúc
        /// (chưa thuộc kỳ học phí nào). Trả về số buổi đã đánh dấu.
        /// Dùng cuối tháng trước khi chốt kỳ — đỡ phải bấm từng buổi.
        /// </summary>
        public async Task<Response<int>> MarkDonePastAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var now = AppTime.VnNow;
            var today = now.Date;
            var nowTime = now.TimeOfDay;

            // Buổi đã qua = ngày < hôm nay, hoặc hôm nay nhưng đã hết giờ học
            var lessons = await _repos.Table
                .Where(l => l.IdTutor == idTutor
                         && l.Status == LessonStatusEnums.Scheduled
                         && l.IdTuitionPeriod == null
                         && (l.ScheduledDate < today
                          || (l.ScheduledDate == today && l.EndTime <= nowTime)))
                .ToListAsync();

            if (lessons.Count == 0)
                return Response<int>.Success(0, StatusCode.Ok.ToDescription());

            foreach (var l in lessons)
            {
                l.Status = LessonStatusEnums.Done;
                l.DoneAt = now;
                l.MarkDirty(nameof(l.Status));
                l.MarkDirty(nameof(l.DoneAt));
            }
            await _unitOfWork.SaveChangesAsync();

            // Buổi đã dạy thì không nhắc nữa
            foreach (var l in lessons)
                await _notiGen.CancelForLessonAsync(l.Id);

            return Response<int>.Success(lessons.Count, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<LessonDto>> CancelAsync(Guid id, string? reason)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var lesson = await _repos.Table.FirstOrDefaultAsync(l => l.Id == id && l.IdTutor == idTutor);
            if (lesson == null) return Response<LessonDto>.Error(StatusCode.NotFound, "Không tìm thấy");
            if (lesson.IdTuitionPeriod.HasValue)
                return Response<LessonDto>.Error(StatusCode.BadRequest, "Buổi học đã chốt vào kỳ học phí");

            lesson.Status = LessonStatusEnums.Cancelled;
            if (!string.IsNullOrEmpty(reason))
                lesson.Notes = string.IsNullOrEmpty(lesson.Notes) ? reason : $"{lesson.Notes}\n[Huỷ] {reason}";
            lesson.MarkDirty(nameof(lesson.Status));
            lesson.MarkDirty(nameof(lesson.Notes));
            await _repos.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            // Buổi đã huỷ thì cancel mọi nhắc còn chờ gửi
            await _notiGen.CancelForLessonAsync(lesson.Id);

            return Response<LessonDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<Lesson, LessonDto>(lesson), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<PagingData<List<LessonDetailDto>>>> GetByFilterAsync(LessonGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = from l in _repos.TableNoTracking
                        join s in _studentRepos.TableNoTracking on l.IdStudent equals s.Id
                        join p in _parentRepos.TableNoTracking on s.IdParent equals p.Id into pj
                        from p in pj.DefaultIfEmpty()
                        join c in _courseRepos.TableNoTracking on l.IdCourse equals c.Id into cj
                        from c in cj.DefaultIfEmpty()
                        where l.IdTutor == idTutor
                        select new { l, s, p, c };

            if (filter.IdStudent.HasValue)
                query = query.Where(x => x.l.IdStudent == filter.IdStudent.Value);
            if (filter.Status.HasValue)
                query = query.Where(x => x.l.Status == filter.Status.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(x => x.l.ScheduledDate >= filter.FromDate.Value.Date);
            if (filter.ToDate.HasValue)
                query = query.Where(x => x.l.ScheduledDate <= filter.ToDate.Value.Date);

            int total = query.Count();
            var items = query.OrderBy(x => x.l.ScheduledDate).ThenBy(x => x.l.StartTime)
                             .Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .AsEnumerable()
                             .Select(x => MapDetail(x.l, x.s, x.p, x.c))
                             .ToList();
            var paging = PagingData<List<LessonDetailDto>>.Create(items, filter.PageNumber, (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<LessonDetailDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<LessonDetailDto>>> GetWeekAsync(DateTime weekStart)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var ws = weekStart.Date;
            var we = ws.AddDays(7);

            var datas = await (from l in _repos.TableNoTracking
                               join s in _studentRepos.TableNoTracking on l.IdStudent equals s.Id
                               join p in _parentRepos.TableNoTracking on s.IdParent equals p.Id into pj
                               from p in pj.DefaultIfEmpty()
                               join c in _courseRepos.TableNoTracking on l.IdCourse equals c.Id into cj
                               from c in cj.DefaultIfEmpty()
                               where l.IdTutor == idTutor && l.ScheduledDate >= ws && l.ScheduledDate < we
                               orderby l.ScheduledDate, l.StartTime
                               select new { l, s, p, c }).ToListAsync();

            var items = datas.Select(x => MapDetail(x.l, x.s, x.p, x.c)).ToList();
            return Response<List<LessonDetailDto>>.Success(items, StatusCode.Ok.ToDescription());
        }

        private static LessonDetailDto MapDetail(Lesson l, Student s, Parent? p, StudentCourse? c = null) => new LessonDetailDto
        {
            Id = l.Id,
            IdTutor = l.IdTutor,
            IdStudent = l.IdStudent,
            IdCourse = l.IdCourse,
            ScheduledDate = l.ScheduledDate,
            StartTime = l.StartTime,
            EndTime = l.EndTime,
            Location = l.Location,
            Status = l.Status,
            ChargeAmount = l.ChargeAmount,
            DoneAt = l.DoneAt,
            Notes = l.Notes,
            IdTuitionPeriod = l.IdTuitionPeriod,
            StudentFullName = s?.FullName,
            ParentPhone = p?.Phone,
            CourseSubject = c?.Subject,
        };
    }
}
