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
    public interface ITuitionPeriodService : IBaseService<TuitionPeriod, TuitionPeriodDto>
    {
        Task<Response<PagingData<List<TuitionPeriodDetailDto>>>> GetByFilterAsync(TuitionPeriodGridFilter filter);
        Task<Response<TuitionPeriodDto>> OpenOrGetAsync(Guid idStudent, int month, int year);
        Task<Response<TuitionPeriodDto>> CloseAsync(Guid id, decimal adjustment, string? notes);
        Task<Response<TuitionPeriodDto>> RecordPaymentAsync(Guid id, decimal amount, string? notes, string? method = null);
        Task<Response<List<TuitionPaymentDto>>> GetPaymentsAsync(Guid idPeriod);
        Task<Response<TuitionPreviewDto>> PreviewAsync(Guid idStudent, int month, int year);
        Task<Response<MonthClosePreviewDto>> PreviewMonthAsync(int month, int year);
        Task<Response<MonthCloseResultDto>> CloseMonthBulkAsync(MonthCloseBulkReq req);
    }

    public class TuitionPeriodService : TutorScopedBaseService<TuitionPeriod, TuitionPeriodDto>, ITuitionPeriodService
    {
        private readonly ITDRepository<Lesson> _lessonRepos;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Parent> _parentRepos;
        private readonly INotificationGenerator _notiGen;
        private readonly ISubscriptionService _subService;

        public TuitionPeriodService(IUnitOfWork unitOfWork, IUserContextService userContext, INotificationGenerator notiGen, ISubscriptionService subService)
            : base(unitOfWork, userContext)
        {
            _lessonRepos = unitOfWork.GetRepository<Lesson>();
            _studentRepos = unitOfWork.GetRepository<Student>();
            _parentRepos = unitOfWork.GetRepository<Parent>();
            _notiGen = notiGen;
            _subService = subService;
        }

        /// <summary>
        /// Bảng chốt kỳ THÁNG: quét HS có buổi Đã dạy chưa chốt trong tháng — hệ thống
        /// tìm sẵn, tutor chỉ duyệt (loại HS có kỳ đã chốt: dùng "Chốt lại" trên card).
        /// </summary>
        public async Task<Response<MonthClosePreviewDto>> PreviewMonthAsync(int month, int year)
        {
            if (month < 1 || month > 12 || year < 2020 || year > 2100)
                return Response<MonthClosePreviewDto>.Error(StatusCode.BadRequest, "Tháng/năm không hợp lệ");

            var idTutor = await GetCurrentTutorIdAsync();

            // Gom buổi Đã dạy chưa thuộc kỳ nào theo HS
            var doneByStudent = await _lessonRepos.TableNoTracking
                .Where(l => l.IdTutor == idTutor
                         && l.Status == LessonStatusEnums.Done
                         && l.ScheduledDate.Year == year && l.ScheduledDate.Month == month
                         && l.IdTuitionPeriod == null)
                .GroupBy(l => l.IdStudent)
                .Select(g => new { IdStudent = g.Key, Count = g.Count(), Total = g.Sum(x => x.ChargeAmount) })
                .ToListAsync();

            // Loại HS có kỳ tháng này ĐÃ CHỐT (tránh đè — chốt bổ sung dùng luồng riêng)
            var closedIds = await _repos.TableNoTracking
                .Where(p => p.IdTutor == idTutor && p.PeriodMonth == month && p.PeriodYear == year && p.ClosedAt != null)
                .Select(p => p.IdStudent)
                .ToListAsync();
            doneByStudent = doneByStudent.Where(x => !closedIds.Contains(x.IdStudent)).ToList();

            var ids = doneByStudent.Select(x => x.IdStudent).ToList();
            var names = await _studentRepos.TableNoTracking
                .Where(s => ids.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.FullName);

            // Cảnh báo: buổi ĐÃ QUA trong tháng còn "Đã lên lịch" (quên đánh dấu → chốt sẽ thiếu)
            var today = AppTime.VnNow.Date;
            var pastScheduled = await _lessonRepos.TableNoTracking
                .CountAsync(l => l.IdTutor == idTutor
                              && l.Status == LessonStatusEnums.Scheduled
                              && l.ScheduledDate.Year == year && l.ScheduledDate.Month == month
                              && l.ScheduledDate < today);

            var dto = new MonthClosePreviewDto
            {
                Month = month,
                Year = year,
                PastScheduledLessons = pastScheduled,
                TotalAmount = doneByStudent.Sum(x => x.Total),
                Candidates = doneByStudent
                    .Select(x => new MonthCloseCandidateDto
                    {
                        IdStudent = x.IdStudent,
                        StudentFullName = names.GetValueOrDefault(x.IdStudent, "—"),
                        DoneLessons = x.Count,
                        TotalAmount = x.Total,
                    })
                    .OrderBy(c => c.StudentFullName)
                    .ToList(),
            };
            return Response<MonthClosePreviewDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Chốt HÀNG LOẠT các HS đã chọn trong tháng: mỗi em = OpenOrGet + Close (tái dùng
        /// luồng đơn lẻ → giữ nguyên validate + sinh nhắc học phí). Lỗi từng em không chặn em khác.
        /// </summary>
        public async Task<Response<MonthCloseResultDto>> CloseMonthBulkAsync(MonthCloseBulkReq req)
        {
            if (req.StudentIds.Count == 0)
                return Response<MonthCloseResultDto>.Error(StatusCode.BadRequest, "Chưa chọn học sinh nào");

            var result = new MonthCloseResultDto();
            foreach (var idStudent in req.StudentIds.Distinct())
            {
                var open = await OpenOrGetAsync(idStudent, req.Month, req.Year);
                if (open.Status != StatusCode.Ok || open.Data == null)
                {
                    result.Errors.Add(open.Message ?? "Không mở được kỳ");
                    continue;
                }
                if (open.Data.ClosedAt.HasValue)
                {
                    result.Errors.Add($"{open.Data.IdStudent}: kỳ đã chốt trước đó");
                    continue;
                }

                var close = await CloseAsync(open.Data.Id!.Value, adjustment: 0, notes: null);
                if (close.Status != StatusCode.Ok || close.Data == null)
                {
                    result.Errors.Add(close.Message ?? "Chốt kỳ thất bại");
                    continue;
                }
                result.ClosedCount++;
                result.TotalAmount += close.Data.FinalAmount;
            }
            return Response<MonthCloseResultDto>.Success(result, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<TuitionPeriodDto>> OpenOrGetAsync(Guid idStudent, int month, int year)
        {
            if (month < 1 || month > 12 || year < 2020 || year > 2100)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Tháng/năm không hợp lệ");

            var idTutor = await GetCurrentTutorIdAsync();

            var subErr = await _subService.CheckCanWriteAsync(idTutor);
            if (subErr != null) return Response<TuitionPeriodDto>.Error(StatusCode.Forbidden, subErr);

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

            var subErr = await _subService.CheckCanWriteAsync(idTutor);
            if (subErr != null) return Response<TuitionPeriodDto>.Error(StatusCode.Forbidden, subErr);

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
            period.ClosedAt = AppTime.VnNow;
            period.Notes = notes;

            period.MarkDirty(nameof(period.TotalLessons));
            period.MarkDirty(nameof(period.TotalAmount));
            period.MarkDirty(nameof(period.Adjustment));
            period.MarkDirty(nameof(period.FinalAmount));
            period.MarkDirty(nameof(period.Status));
            period.MarkDirty(nameof(period.ClosedAt));
            period.MarkDirty(nameof(period.Notes));

            await _unitOfWork.SaveChangesAsync();

            // Sinh nhắc thu tiền cho phụ huynh (1 lần khi vừa chốt kỳ)
            await _notiGen.GenerateForTuitionPeriodAsync(period);

            return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(period), StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Ghi nhận thanh toán (1 phần hoặc đủ): cộng dồn PaidAmount + LƯU 1 DÒNG
        /// LỊCH SỬ TuitionPayment (số thực ghi nhận, hình thức, ghi chú).
        /// </summary>
        public async Task<Response<TuitionPeriodDto>> RecordPaymentAsync(Guid id, decimal amount, string? notes, string? method = null)
        {
            if (amount <= 0)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Số tiền phải > 0");

            var idTutor = await GetCurrentTutorIdAsync();
            var period = await _repos.Table.FirstOrDefaultAsync(t => t.Id == id && t.IdTutor == idTutor);
            if (period == null)
                return Response<TuitionPeriodDto>.Error(StatusCode.NotFound, "Không tìm thấy kỳ học phí");
            if (!period.ClosedAt.HasValue)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Kỳ chưa chốt — chốt trước khi thu tiền");

            // Số THỰC ghi nhận: clamp về phần còn nợ (nhập dư không ghi dư)
            var credited = Math.Min(amount, Math.Max(0, period.FinalAmount - period.PaidAmount));
            if (credited <= 0)
                return Response<TuitionPeriodDto>.Error(StatusCode.BadRequest, "Kỳ này đã thu đủ");

            period.PaidAmount += credited;
            period.Status = period.PaidAmount >= period.FinalAmount
                ? TuitionPeriodStatusEnums.Paid
                : TuitionPeriodStatusEnums.PartialPaid;

            period.MarkDirty(nameof(period.PaidAmount));
            period.MarkDirty(nameof(period.Status));

            // Lịch sử: mỗi đợt thu 1 dòng — sổ sách soi lại được từng lần
            var paymentRepos = _unitOfWork.GetRepository<TuitionPayment>();
            await paymentRepos.CreateAsync(new TuitionPayment
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                IdPeriod = period.Id,
                Amount = credited,
                Method = method,
                Notes = notes,
            });

            await _unitOfWork.SaveChangesAsync();
            return Response<TuitionPeriodDto>.Success(TD.Lib.AutoMapper.AutoMapperGeneric.Map<TuitionPeriod, TuitionPeriodDto>(period), StatusCode.Ok.ToDescription());
        }

        /// <summary>Lịch sử các đợt thu của 1 kỳ — mới nhất trước.</summary>
        public async Task<Response<List<TuitionPaymentDto>>> GetPaymentsAsync(Guid idPeriod)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var list = await _unitOfWork.GetRepository<TuitionPayment>().TableNoTracking
                .Where(p => p.IdPeriod == idPeriod && p.IdTutor == idTutor)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
            return Response<List<TuitionPaymentDto>>.Success(
                TD.Lib.AutoMapper.AutoMapperGeneric.Map<List<TuitionPayment>, List<TuitionPaymentDto>>(list),
                StatusCode.Ok.ToDescription());
        }

        /// <summary>Preview số buổi Done + tổng tiền chưa chốt — KHÔNG sửa DB.</summary>
        public async Task<Response<TuitionPreviewDto>> PreviewAsync(Guid idStudent, int month, int year)
        {
            if (month < 1 || month > 12 || year < 2020 || year > 2100)
                return Response<TuitionPreviewDto>.Error(StatusCode.BadRequest, "Tháng/năm không hợp lệ");

            var idTutor = await GetCurrentTutorIdAsync();
            var studentOk = await _studentRepos.TableNoTracking
                .AnyAsync(s => s.Id == idStudent && s.IdTutor == idTutor);
            if (!studentOk)
                return Response<TuitionPreviewDto>.Error(StatusCode.BadRequest, "Học sinh không hợp lệ");

            var courseRepos = _unitOfWork.GetRepository<StudentCourse>();
            var classRepos = _unitOfWork.GetRepository<ClassRoom>();
            var lessons = await (from l in _lessonRepos.TableNoTracking
                                 join c in courseRepos.TableNoTracking on l.IdCourse equals c.Id into cj
                                 from c in cj.DefaultIfEmpty()
                                 join k in classRepos.TableNoTracking on l.IdClass equals k.Id into kj
                                 from k in kj.DefaultIfEmpty()
                                 where l.IdTutor == idTutor
                                    && l.IdStudent == idStudent
                                    && l.Status == LessonStatusEnums.Done
                                    && l.ScheduledDate.Year == year
                                    && l.ScheduledDate.Month == month
                                    && l.IdTuitionPeriod == null
                                 orderby l.ScheduledDate, l.StartTime
                                 select new TuitionPreviewLineDto
                                 {
                                     IdLesson = l.Id,
                                     ScheduledDate = l.ScheduledDate,
                                     StartTime = l.StartTime.ToString(@"hh\:mm"),
                                     EndTime = l.EndTime.ToString(@"hh\:mm"),
                                     ChargeAmount = l.ChargeAmount,
                                     // Môn: từ đăng ký 1-1, hoặc môn của lớp (buổi sinh từ ClassRoom)
                                     Subject = c != null ? c.Subject : (k != null ? k.Subject : null),
                                 }).ToListAsync();

            // Đếm buổi còn "Đã lên lịch" trong tháng — để FE giải thích vì sao 0 buổi Done
            var scheduledCount = await _lessonRepos.TableNoTracking
                .CountAsync(l => l.IdTutor == idTutor
                              && l.IdStudent == idStudent
                              && l.Status == LessonStatusEnums.Scheduled
                              && l.ScheduledDate.Year == year
                              && l.ScheduledDate.Month == month
                              && l.IdTuitionPeriod == null);

            var preview = new TuitionPreviewDto
            {
                IdStudent = idStudent,
                PeriodMonth = month,
                PeriodYear = year,
                TotalLessons = lessons.Count,
                TotalAmount = lessons.Sum(x => x.ChargeAmount),
                ScheduledLessons = scheduledCount,
                Lessons = lessons,
            };
            return Response<TuitionPreviewDto>.Success(preview, StatusCode.Ok.ToDescription());
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
