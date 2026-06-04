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

namespace EduTrack.API.Services.TutorDomain
{
    public interface INotificationService : IBaseService<Notification, NotificationDto>
    {
        Task<Response<PagingData<List<NotificationDto>>>> GetInboxAsync(int pageNumber = 1, int pageSize = 20);
        Task<Response<PagingData<List<NotificationDto>>>> GetByFilterAsync(NotificationGridFilter filter);
        Task<Response<NotificationDto>> MarkSentAsync(Guid id);
        Task<Response<int>> GetDueCountAsync();
    }

    public class NotificationService : TutorScopedBaseService<Notification, NotificationDto>, INotificationService
    {
        public NotificationService(IUnitOfWork uow, IUserContextService ctx) : base(uow, ctx) { }

        /// <summary>
        /// Inbox = "việc cần làm BÂY GIỜ":
        /// - Pending mà <see cref="Notification.ScheduledAt"/> ≤ now (đã đến giờ nhắc)
        /// - Hoặc đã Sent trong 7 ngày (cho tutor xem lịch sử gần)
        /// Sort: Pending trước, theo ScheduledAt cũ nhất. PAGING (load-more ở FE) —
        /// thay Take(100) cứng cũ vốn nuốt âm thầm nhắc thứ 101 trở đi.
        /// </summary>
        public async Task<Response<PagingData<List<NotificationDto>>>> GetInboxAsync(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var idTutor = await GetCurrentTutorIdAsync();
            var now = AppTime.VnNow;
            var since = now.AddDays(-7);

            var query = _repos.TableNoTracking
                .Where(n => n.IdTutor == idTutor
                         && ((n.Status == NotificationStatusEnums.Pending && n.ScheduledAt <= now)
                          || (n.Status == NotificationStatusEnums.Sent && n.SentAt >= since)));

            int total = await query.CountAsync();
            var list = await query
                .OrderBy(n => n.Status) // Pending trước
                .ThenBy(n => n.ScheduledAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paging = PagingData<List<NotificationDto>>.Create(
                AutoMapperGeneric.Map<List<Notification>, List<NotificationDto>>(list),
                pageNumber, (int)Math.Ceiling((double)total / pageSize), total);
            return Response<PagingData<List<NotificationDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        /// <summary>Số nhắc Pending đã đến hạn — cho badge sidebar (query nhẹ, gọi thường xuyên).</summary>
        public async Task<Response<int>> GetDueCountAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var now = AppTime.VnNow;
            var count = await _repos.TableNoTracking
                .CountAsync(n => n.IdTutor == idTutor
                              && n.Status == NotificationStatusEnums.Pending
                              && n.ScheduledAt <= now);
            return Response<int>.Success(count, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<PagingData<List<NotificationDto>>>> GetByFilterAsync(NotificationGridFilter filter)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var query = _repos.TableNoTracking.Where(n => n.IdTutor == idTutor);

            if (filter.Status.HasValue) query = query.Where(n => n.Status == filter.Status.Value);
            if (filter.Type.HasValue) query = query.Where(n => n.Type == filter.Type.Value);
            if (filter.IdStudent.HasValue) query = query.Where(n => n.IdStudent == filter.IdStudent.Value);

            int total = query.Count();
            var items = query.OrderByDescending(n => n.DateCreated)
                             .Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .Select(n => AutoMapperGeneric.Map<Notification, NotificationDto>(n))
                             .ToList();
            var paging = PagingData<List<NotificationDto>>.Create(items, filter.PageNumber, (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<NotificationDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<NotificationDto>> MarkSentAsync(Guid id)
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var noti = await _repos.Table.FirstOrDefaultAsync(n => n.Id == id && n.IdTutor == idTutor);
            if (noti == null) return Response<NotificationDto>.Error(StatusCode.NotFound, "Không tìm thấy");
            if (noti.Status != NotificationStatusEnums.Pending)
                return Response<NotificationDto>.Error(StatusCode.BadRequest, "Nhắc này không ở trạng thái chờ gửi");

            noti.Status = NotificationStatusEnums.Sent;
            noti.SentAt = AppTime.VnNow;
            noti.MarkDirty(nameof(noti.Status));
            noti.MarkDirty(nameof(noti.SentAt));
            await _unitOfWork.SaveChangesAsync();

            return Response<NotificationDto>.Success(AutoMapperGeneric.Map<Notification, NotificationDto>(noti), StatusCode.Ok.ToDescription());
        }
    }
}
