using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    /// <summary>
    /// Founder-level services — chỉ super admin truy cập. Controller phải check IsSuper trước khi gọi.
    /// </summary>
    public interface IAdminService
    {
        Task<Response<PagingData<List<TutorWithSubDto>>>> GetTutorsAsync(AdminTutorFilter filter);
        Task<Response<TutorWithSubDto>> ConfirmPaymentAsync(ConfirmPaymentReq req, Guid adminId);
        Task<Response<AdminStatsDto>> GetStatsAsync();
        Task<Response<PagingData<List<AdminPaymentRow>>>> GetPaymentsAsync(GridFilterBase filter);
    }

    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly ISubscriptionService _subService;

        private readonly ITDRepository<User> _userRepos;
        private readonly ITDRepository<Role> _roleRepos;
        private readonly ITDRepository<UserRole> _userRoleRepos;
        private readonly ITDRepository<Subscription> _subRepos;
        private readonly ITDRepository<SubscriptionPayment> _paymentRepos;
        private readonly ITDRepository<Student> _studentRepos;
        private readonly ITDRepository<Lesson> _lessonRepos;

        public AdminService(IUnitOfWork uow, ISubscriptionService subService)
        {
            _uow = uow;
            _subService = subService;
            _userRepos = uow.GetRepository<User>();
            _roleRepos = uow.GetRepository<Role>();
            _userRoleRepos = uow.GetRepository<UserRole>();
            _subRepos = uow.GetRepository<Subscription>();
            _paymentRepos = uow.GetRepository<SubscriptionPayment>();
            _studentRepos = uow.GetRepository<Student>();
            _lessonRepos = uow.GetRepository<Lesson>();
        }

        public async Task<Response<PagingData<List<TutorWithSubDto>>>> GetTutorsAsync(AdminTutorFilter filter)
        {
            // Lấy id của Role TUTOR
            var tutorRoleId = await _roleRepos.TableNoTracking
                .Where(r => r.Code == Commons.RoleCodes.Tutor)
                .Select(r => r.Id).FirstOrDefaultAsync();
            if (tutorRoleId == Guid.Empty)
                return Response<PagingData<List<TutorWithSubDto>>>.Error(StatusCode.InternalServerError, "Chưa cấu hình Role TUTOR");

            // Join User + UserRole + Subscription
            var baseQuery = from u in _userRepos.TableNoTracking
                            join ur in _userRoleRepos.TableNoTracking on u.Id equals ur.IdUser
                            where ur.IdRole == tutorRoleId
                            join s in _subRepos.TableNoTracking on u.Id equals s.IdTutor into subJoin
                            from s in subJoin.DefaultIfEmpty()
                            select new { User = u, Sub = s };

            if (filter.Plan.HasValue)
                baseQuery = baseQuery.Where(x => x.Sub != null && x.Sub.Plan == filter.Plan.Value);
            if (filter.Status.HasValue)
                baseQuery = baseQuery.Where(x => x.Sub != null && x.Sub.Status == filter.Status.Value);
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    x.User.UserName.ToLower().Contains(kw)
                    || x.User.DisplayName.ToLower().Contains(kw));
            }

            int total = baseQuery.Count();
            var items = await baseQuery
                .OrderByDescending(x => x.User.DateCreated)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Tính thêm stats per tutor — gom batch để tránh N+1
            var tutorIds = items.Select(x => x.User.Id).ToList();
            var studentCounts = await _studentRepos.TableNoTracking
                .Where(s => tutorIds.Contains(s.IdTutor))
                .GroupBy(s => s.IdTutor)
                .Select(g => new { IdTutor = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdTutor, x => x.Count);

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var lessonCountsThisMonth = await _lessonRepos.TableNoTracking
                .Where(l => tutorIds.Contains(l.IdTutor) && l.ScheduledDate >= monthStart)
                .GroupBy(l => l.IdTutor)
                .Select(g => new { IdTutor = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IdTutor, x => x.Count);

            var revenueLifetime = await _paymentRepos.TableNoTracking
                .Where(p => tutorIds.Contains(p.IdTutor) && p.Status == PaymentStatusEnums.Confirmed)
                .GroupBy(p => p.IdTutor)
                .Select(g => new { IdTutor = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.IdTutor, x => x.Total);

            var rows = items.Select(x =>
            {
                int? days = null;
                if (x.Sub != null)
                {
                    var target = x.Sub.Status == SubscriptionStatusEnums.Trial
                        ? x.Sub.TrialEndsAt
                        : x.Sub.CurrentPeriodEnd ?? now;
                    days = (int)Math.Ceiling((target - now).TotalDays);
                }
                return new TutorWithSubDto
                {
                    IdTutor = x.User.Id,
                    UserName = x.User.UserName ?? "",
                    DisplayName = x.User.DisplayName ?? "",
                    Email = x.User.Email,
                    SignupAt = x.User.DateCreated,
                    IdSubscription = x.Sub?.Id,
                    Plan = x.Sub?.Plan ?? PlanCodeEnums.Free,
                    Status = x.Sub?.Status ?? SubscriptionStatusEnums.Expired,
                    TrialEndsAt = x.Sub?.TrialEndsAt,
                    CurrentPeriodEnd = x.Sub?.CurrentPeriodEnd,
                    DaysRemaining = days,
                    StudentCount = studentCounts.GetValueOrDefault(x.User.Id),
                    LessonCountThisMonth = lessonCountsThisMonth.GetValueOrDefault(x.User.Id),
                    RevenueLifetime = revenueLifetime.GetValueOrDefault(x.User.Id),
                };
            }).ToList();

            var paging = PagingData<List<TutorWithSubDto>>.Create(rows, filter.PageNumber,
                (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<TutorWithSubDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<TutorWithSubDto>> ConfirmPaymentAsync(ConfirmPaymentReq req, Guid adminId)
        {
            if (req.Months <= 0 || req.Months > 24)
                return Response<TutorWithSubDto>.Error(StatusCode.BadRequest, "Số tháng phải 1-24");
            if (req.Amount <= 0)
                return Response<TutorWithSubDto>.Error(StatusCode.BadRequest, "Số tiền phải > 0");

            var user = await _userRepos.TableNoTracking.FirstOrDefaultAsync(u => u.Id == req.IdTutor);
            if (user == null)
                return Response<TutorWithSubDto>.Error(StatusCode.NotFound, "Không tìm thấy gia sư");

            // Đảm bảo có Subscription (nếu chưa, tạo Trial)
            var sub = await _subService.EnsureTrialAsync(req.IdTutor);

            // Insert SubscriptionPayment
            var payment = new SubscriptionPayment
            {
                Id = Guid.NewGuid(),
                IdTutor = req.IdTutor,
                IdSubscription = sub.Id,
                Plan = req.Plan,
                Months = req.Months,
                Amount = req.Amount,
                Status = PaymentStatusEnums.Confirmed,
                TransferRef = req.TransferRef,
                Notes = req.Notes,
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedByAdmin = adminId,
            };
            await _paymentRepos.CreateAsync(payment);
            await _uow.SaveChangesAsync();

            // Extend subscription
            await _subService.ExtendAsync(sub.Id, req.Plan, req.Months);

            // Refetch
            var refreshed = await _subRepos.TableNoTracking.FirstOrDefaultAsync(s => s.Id == sub.Id);
            var now = DateTime.UtcNow;
            var days = refreshed?.CurrentPeriodEnd.HasValue == true
                ? (int)Math.Ceiling((refreshed.CurrentPeriodEnd!.Value - now).TotalDays)
                : (int?)null;

            var dto = new TutorWithSubDto
            {
                IdTutor = user.Id,
                UserName = user.UserName ?? "",
                DisplayName = user.DisplayName ?? "",
                Email = user.Email,
                SignupAt = user.DateCreated,
                IdSubscription = refreshed?.Id,
                Plan = refreshed?.Plan ?? req.Plan,
                Status = refreshed?.Status ?? SubscriptionStatusEnums.Active,
                TrialEndsAt = refreshed?.TrialEndsAt,
                CurrentPeriodEnd = refreshed?.CurrentPeriodEnd,
                DaysRemaining = days,
            };
            return Response<TutorWithSubDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<AdminStatsDto>> GetStatsAsync()
        {
            var tutorRoleId = await _roleRepos.TableNoTracking
                .Where(r => r.Code == Commons.RoleCodes.Tutor)
                .Select(r => r.Id).FirstOrDefaultAsync();

            var tutorIds = await _userRoleRepos.TableNoTracking
                .Where(ur => ur.IdRole == tutorRoleId)
                .Select(ur => ur.IdUser).ToListAsync();

            var subs = await _subRepos.TableNoTracking
                .Where(s => tutorIds.Contains(s.IdTutor))
                .ToListAsync();

            // Update Status="Expired" trên-the-fly cho subs trial đã hết hạn (tính tại runtime, không lưu)
            var now = DateTime.UtcNow;
            int trial = 0, active = 0, expired = 0, cancelled = 0;
            decimal mrr = 0;
            foreach (var s in subs)
            {
                var effectiveStatus = s.Status;
                if (effectiveStatus == SubscriptionStatusEnums.Trial && s.TrialEndsAt < now)
                    effectiveStatus = SubscriptionStatusEnums.Expired;
                if (effectiveStatus == SubscriptionStatusEnums.Active && (s.CurrentPeriodEnd == null || s.CurrentPeriodEnd < now))
                    effectiveStatus = SubscriptionStatusEnums.Expired;

                switch (effectiveStatus)
                {
                    case SubscriptionStatusEnums.Trial:     trial++; break;
                    case SubscriptionStatusEnums.Active:    active++; mrr += _subService.PriceOf(s.Plan); break;
                    case SubscriptionStatusEnums.Expired:   expired++; break;
                    case SubscriptionStatusEnums.Cancelled: cancelled++; break;
                }
            }

            var monthStart = new DateTime(now.Year, now.Month, 1);
            var signupsThisMonth = await _userRepos.TableNoTracking
                .Where(u => tutorIds.Contains(u.Id) && u.DateCreated >= monthStart)
                .CountAsync();

            var revenueThisMonth = await _paymentRepos.TableNoTracking
                .Where(p => p.Status == PaymentStatusEnums.Confirmed && p.ConfirmedAt >= monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var revenueLifetime = await _paymentRepos.TableNoTracking
                .Where(p => p.Status == PaymentStatusEnums.Confirmed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Response<AdminStatsDto>.Success(new AdminStatsDto
            {
                TotalTutors = tutorIds.Count,
                TrialCount = trial,
                ActiveCount = active,
                ExpiredCount = expired,
                CancelledCount = cancelled,
                SignupsThisMonth = signupsThisMonth,
                MrrEstimate = mrr,
                RevenueThisMonth = revenueThisMonth,
                RevenueLifetime = revenueLifetime,
            }, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<PagingData<List<AdminPaymentRow>>>> GetPaymentsAsync(GridFilterBase filter)
        {
            var query = from p in _paymentRepos.TableNoTracking
                        join u in _userRepos.TableNoTracking on p.IdTutor equals u.Id
                        select new { p, u };

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                query = query.Where(x => x.u.UserName!.ToLower().Contains(kw)
                                       || x.u.DisplayName!.ToLower().Contains(kw));
            }

            int total = query.Count();
            var rows = await query
                .OrderByDescending(x => x.p.ConfirmedAt ?? x.p.DateCreated)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new AdminPaymentRow
                {
                    Id = x.p.Id,
                    IdTutor = x.p.IdTutor,
                    TutorDisplayName = x.u.DisplayName ?? "",
                    TutorUserName = x.u.UserName ?? "",
                    Plan = x.p.Plan,
                    Months = x.p.Months,
                    Amount = x.p.Amount,
                    TransferRef = x.p.TransferRef,
                    Notes = x.p.Notes,
                    ConfirmedAt = x.p.ConfirmedAt,
                }).ToListAsync();

            var paging = PagingData<List<AdminPaymentRow>>.Create(rows, filter.PageNumber,
                (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<AdminPaymentRow>>>.Success(paging, StatusCode.Ok.ToDescription());
        }
    }
}
