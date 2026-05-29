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
    public interface ITuitionPeriodService : IBaseService<TuitionPeriod, TuitionPeriodDto>
    {
        Task<Response<PagingData<List<TuitionPeriodDetailDto>>>> GetByFilterAsync(TuitionPeriodGridFilter filter);
        Task<Response<TuitionPeriodDto>> OpenOrGetAsync(Guid idStudent, int month, int year);
        Task<Response<TuitionPeriodDto>> CloseAsync(Guid id, decimal adjustment, string? notes);
        Task<Response<TuitionPeriodDto>> RecordPaymentAsync(Guid id, decimal amount, string? notes);
    }

    public class TuitionPeriodService : TutorScopedBaseService<TuitionPeriod, TuitionPeriodDto>, ITuitionPeriodService
    {
        private readonly ITDRepository<Lesson> _lessonRepos;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Parent> _parentRepos;

        public TuitionPeriodService(IUnitOfWork unitOfWork, IUserContextService userContext)
            : base(unitOfWork, userContext)
        {
            _lessonRepos = unitOfWork.GetRepository<Lesson>();
            _studentRepos = unitOfWork.GetRepository<Student>();
            _parentRepos = unitOfWork.GetRepository<Parent>();
        }

        public async Task<Response<TuitionPeriodDto>> OpenOrGetAsync(Guid idStudent, int month, int year)
        {
            if (month < 1 || month > 12 || year < 2020 || year > 2100)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Tháng/năm không hợp lệ");

            var idTutor = await GetCurrentTutorIdAsync();
            var studentOk = await _studentRepos.TableNoTracking
                .AnyAsync(s => s.Id == idStudent && s.IdTutor == idTutor);
            if (!studentOk)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Học sinh không hợp lệ");

            // Đã có kỳ này — trả về
            var existed = await _repos.Table.FirstOrDefaultAsync(t =>
                t.IdStudent == idStudent && t.PeriodMonth == month && t.PeriodYear == year);
            if (existed != null)
                return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(existed), StatusCode.Ok.ToDescription());

            // Tạo kỳ mới (Open, chưa Close)
            var entity = new TuitionPeriod
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                IdStudent = idStudent,
                PeriodMonth = month,
                PeriodYear = year,
                Status = TuitionPeriodStatusEnums.Open,
            };
            await _repos.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(entity), StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Chốt kỳ học phí: gom mọi Lesson Done của HS trong tháng-năm, set IdTuitionPeriod, tính tổng.
        /// </summary>
        public async Task<Response<TuitionPeriodDto>> CloseAsync(Guid id, decimal adjustment, string? notes)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var period = await _repos.Table.FirstOrDefaultAsync(t => t.Id == id && t.IdTutor == idTutor);
            if (period == null)
                return Response<TuitionPeriodDto>.Error(StatusCode.NotFound, "Không tìm thấy kỳ học phí");
            if (period.ClosedAt.HasValue)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Kỳ học phí đã được chốt trước đó");

            // Lấy lesson Done của HS trong tháng-năm này, chưa thuộc kỳ nào
            var lessons = await _lessonRepos.Table
                .Where(l => l.IdTutor == idTutor
                         && l.IdStudent == period.IdStudent
                         && l.Status == LessonStatusEnums.Done
                         && l.ScheduledDate.Year == period.PeriodYear
                         && l.ScheduledDate.Month == period.PeriodMonth
                         && l.IdTuitionPeriod == null)
                .ToListAsync();

            decimal total = lessons.Sum(l => l.ChargeAmount);

            // Lock các Lesson vào period
            foreach (var l in lessons)
            {
                l.IdTuitionPeriod = period.Id;
                l.MarkDirty(nameof(l.IdTuitionPeriod));
            }

            period.TotalLessons = lessons.Count;
            period.TotalAmount = total;
            period.Adjustment = adjustment;
            period.FinalAmount = total + adjustment;
            period.PaidAmount = 0;
            period.Status = TuitionPeriodStatusEnums.Closed;
            period.ClosedAt = DateTime.UtcNow;
            period.Notes = notes;

            period.MarkDirty(nameof(period.TotalLessons));
            period.MarkDirty(nameof(period.TotalAmount));
            period.MarkDirty(nameof(period.Adjustment));
            period.MarkDirty(nameof(period.FinalAmount));
            period.MarkDirty(nameof(period.Status));
            period.MarkDirty(nameof(period.ClosedAt));
            period.MarkDirty(nameof(period.Notes));

            await _unitOfWork.SaveChangesAsync();
            return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(period), StatusCode.Ok.ToDescription());
        }

        /// <summary>Ghi nhận thanh toán (1 phần hoặc đủ). Cộng dồn vào PaidAmount.</summary>
        public async Task<Response<TuitionPeriodDto>> RecordPaymentAsync(Guid id, decimal amount, string? notes)
        {
            if (amount <= 0)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Số tiền phải > 0");

            var idTutor = await GetCurrentTutorIdAsync();
            var period = await _repos.Table.FirstOrDefaultAsync(t => t.Id == id && t.IdTutor == idTutor);
            if (period == null)
                return Response<TuitionPeriodDto>.Error(StatusCode.NotFound, "Không tìm thấy kỳ học phí");
            if (!period.ClosedAt.HasValue)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Kỳ chưa chốt — chốt trước khi thu tiền");

            period.PaidAmount += amount;
            if (period.PaidAmount >= period.FinalAmount)
            {
                period.PaidAmount = period.FinalAmount;
                period.Status = TuitionPeriodStatusEnums.Paid;
            }
            else
            {
                period.Status = TuitionPeriodStatusEnums.PartialPaid;
            }
            if (!string.IsNullOrEmpty(notes))
                period.Notes = string.IsNullOrEmpty(period.Notes) ? notes : $"{period.Notes}\n[Thu {amount:N0}] {notes}";

            period.MarkDirty(nameof(period.PaidAmount));
            period.MarkDirty(nameof(period.Status));
            period.MarkDirty(nameof(period.Notes));

            await _unitOfWork.SaveChangesAsync();
            return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(period), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<PagingData<List<TuitionPeriodDetailDto>>>> GetByFilterAsync(TuitionPeriodGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = from t in _repos.TableNoTracking
                        join s in _studentRepos.TableNoTracking on t.IdStudent equals s.Id
                        join p in _parentRepos.TableNoTracking on s.IdParent equals p.Id into pj
                        from p in pj.DefaultIfEmpty()
                        where t.IdTutor == idTutor
                        select new { t, s, p };

            if (filter.IdStudent.HasValue) query = query.Where(x => x.t.IdStudent == filter.IdStudent.Value);
            if (filter.Status.HasValue) query = query.Where(x => x.t.Status == filter.Status.Value);
            if (filter.PeriodMonth.HasValue) query = query.Where(x => x.t.PeriodMonth == filter.PeriodMonth.Value);
            if (filter.PeriodYear.HasValue) query = query.Where(x => x.t.PeriodYear == filter.PeriodYear.Value);

            int total = query.Count();
            var items = query.OrderByDescending(x => x.t.PeriodYear).ThenByDescending(x => x.t.PeriodMonth)
                             .Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .Select(x => new TuitionPeriodDetailDto
                             {
                                 Id = x.t.Id,
                                 IdTutor = x.t.IdTutor,
                                 IdStudent = x.t.IdStudent,
                                 PeriodMonth = x.t.PeriodMonth,
                                 PeriodYear = x.t.PeriodYear,
                                 ClosedAt = x.t.ClosedAt,
                                 TotalLessons = x.t.TotalLessons,
                                 TotalAmount = x.t.TotalAmount,
                                 Adjustment = x.t.Adjustment,
                                 FinalAmount = x.t.FinalAmount,
                                 PaidAmount = x.t.PaidAmount,
                                 OutstandingAmount = x.t.FinalAmount - x.t.PaidAmount,
                                 Status = x.t.Status,
                                 Notes = x.t.Notes,
                                 StudentFullName = x.s.FullName,
                                 ParentFullName = x.p != null ? x.p.FullName : null,
                                 ParentPhone = x.p != null ? x.p.Phone : null,
                             })
                             .ToList();
            var paging = PagingData<List<TuitionPeriodDetailDto>>.Create(items, filter.PageNumber, (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<TuitionPeriodDetailDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }
    }
}
