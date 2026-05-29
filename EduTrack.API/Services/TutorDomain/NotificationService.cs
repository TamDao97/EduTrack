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
        Task<Response<List<NotificationDto>>> GetInboxAsync();
        Task<Response<PagingData<List<NotificationDto>>>> GetByFilterAsync(NotificationGridFilter filter);
        Task<Response<NotificationDto>> MarkSentAsync(Guid id);
    }

    public class NotificationService : TutorScopedBaseService<Notification, NotificationDto>, INotificationService
    {
        public NotificationService(IUnitOfWork uow, IUserContextService ctx) : base(uow, ctx) { }

        /// <summary>Inbox: nhắc còn Pending hoặc đã Sent trong 7 ngày, sort sớm nhất trước.</summary>
        public async Task<Response<List<NotificationDto>>> GetInboxAsync()
        {
            var idTutor = await GetCurrentTutorIdAsync();
            var since = DateTime.UtcNow.AddDays(-7);

            var list = await _repos.TableNoTracking
                .Where(n => n.IdTutor == idTutor
                         && (n.Status == NotificationStatusEnums.Pending
                          || (n.Status == NotificationStatusEnums.Sent && n.SentAt >= since)))
                .OrderBy(n => n.Status) // Pending trước
                .ThenBy(n => n.ScheduledAt)
                .Take(100)
                .ToListAsync();

            return Response<List<NotificationDto>>.Success(
                AutoMapperGeneric.Map<List<Notification>, List<NotificationDto>>(list),
                StatusCode.Ok.ToDescription());
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
            noti.SentAt = DateTime.UtcNow;
            noti.MarkDirty(nameof(noti.Status));
            noti.MarkDirty(nameof(noti.SentAt));
            await _unitOfWork.SaveChangesAsync();

            return Response<NotificationDto>.Success(AutoMapperGeneric.Map<Notification, NotificationDto>(noti), StatusCode.Ok.ToDescription());
        }
    }
}
