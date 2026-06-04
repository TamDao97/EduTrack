using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto.TutorDomain;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.DataContext.Entity.TutorDomain;
using EduTrack.API.DataContext.Enums;
using EduTrack.API.Services.Base;
using EduTrack.API.Services.Common;
using EduTrack.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services.TutorDomain
{
    public interface IFeedbackService : IBaseService<Feedback, FeedbackDto>
    {
        Task<Response<FeedbackDto>> CreateMyAsync(FeedbackDto dto);
        Task<Response<PagingData<List<FeedbackDto>>>> GetMineAsync(int pageNumber = 1, int pageSize = 10);
        Task<Response<PagingData<List<FeedbackDetailDto>>>> GetByFilterAsync(FeedbackGridFilter filter);
        Task<Response<FeedbackDto>> UpdateStatusAsync(Guid id, FeedbackUpdateStatusReq req);
        Task<Response<int>> GetNewCountAsync();
    }

    public class FeedbackService : TutorScopedBaseService<Feedback, FeedbackDto>, IFeedbackService
    {
        private readonly ITDRepository<User> _userRepos;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IUnitOfWork uow, IUserContextService ctx, IEmailService emailService,
            IConfiguration config, ILogger<FeedbackService> logger) : base(uow, ctx)
        {
            _userRepos = uow.GetRepository<User>();
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        /// <summary>Tutor gửi góp ý mới. Email báo founder (best-effort, không chặn flow).</summary>
        public async Task<Response<FeedbackDto>> CreateMyAsync(FeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Response<FeedbackDto>.Error(StatusCode.BadRequest, "Vui lòng nhập tiêu đề");
            if (string.IsNullOrWhiteSpace(dto.Content))
                return Response<FeedbackDto>.Error(StatusCode.BadRequest, "Vui lòng nhập nội dung");
            if (dto.Rating.HasValue && (dto.Rating < 1 || dto.Rating > 5))
                return Response<FeedbackDto>.Error(StatusCode.BadRequest, "Đánh giá phải từ 1 đến 5 sao");

            var idTutor = await GetCurrentTutorIdAsync();
            var entity = new Feedback
            {
                Id = Guid.NewGuid(),
                IdTutor = idTutor,
                Type = dto.Type,
                Rating = dto.Rating,
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                Status = FeedbackStatusEnums.Moi,
            };
            await _repos.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await TryNotifyFounderAsync(entity);

            return Response<FeedbackDto>.Success(
                AutoMapperGeneric.Map<Feedback, FeedbackDto>(entity), StatusCode.Ok.ToDescription());
        }

        /// <summary>Góp ý của tutor — paging (load-more ở FE), thay Take(100) cứng.</summary>
        public async Task<Response<PagingData<List<FeedbackDto>>>> GetMineAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            pageSize = Math.Clamp(pageSize, 1, 50);

            var idTutor = await GetCurrentTutorIdAsync();
            var query = _repos.TableNoTracking.Where(f => f.IdTutor == idTutor);

            int total = await query.CountAsync();
            var list = await query
                .OrderByDescending(f => f.DateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paging = PagingData<List<FeedbackDto>>.Create(
                AutoMapperGeneric.Map<List<Feedback>, List<FeedbackDto>>(list),
                pageNumber, (int)Math.Ceiling((double)total / pageSize), total);
            return Response<PagingData<List<FeedbackDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        /// <summary>Admin (IsSuper) xem TOÀN BỘ góp ý — không filter theo tutor.</summary>
        public async Task<Response<PagingData<List<FeedbackDetailDto>>>> GetByFilterAsync(FeedbackGridFilter filter)
        {
            var query = from f in _repos.TableNoTracking
                        join u in _userRepos.TableNoTracking on f.IdTutor equals u.Id into uj
                        from u in uj.DefaultIfEmpty()
                        select new { f, u };

            if (filter.Type.HasValue) query = query.Where(x => x.f.Type == filter.Type.Value);
            if (filter.Status.HasValue) query = query.Where(x => x.f.Status == filter.Status.Value);

            int total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.f.DateCreated)
                                   .Skip((filter.PageNumber - 1) * filter.PageSize)
                                   .Take(filter.PageSize)
                                   .Select(x => new FeedbackDetailDto
                                   {
                                       Id = x.f.Id,
                                       IdTutor = x.f.IdTutor,
                                       Type = x.f.Type,
                                       Rating = x.f.Rating,
                                       Title = x.f.Title,
                                       Content = x.f.Content,
                                       Status = x.f.Status,
                                       AdminNote = x.f.AdminNote,
                                       DateCreated = x.f.DateCreated,
                                       TutorDisplayName = x.u != null ? x.u.DisplayName : null,
                                       TutorUserName = x.u != null ? x.u.UserName : null,
                                   })
                                   .ToListAsync();
            var paging = PagingData<List<FeedbackDetailDto>>.Create(items, filter.PageNumber,
                (int)Math.Ceiling((double)total / filter.PageSize), total);
            return Response<PagingData<List<FeedbackDetailDto>>>.Success(paging, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<FeedbackDto>> UpdateStatusAsync(Guid id, FeedbackUpdateStatusReq req)
        {
            var entity = await _repos.Table.FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
                return Response<FeedbackDto>.Error(StatusCode.NotFound, "Không tìm thấy góp ý");

            entity.Status = req.Status;
            entity.AdminNote = req.AdminNote;
            entity.MarkDirty(nameof(entity.Status));
            entity.MarkDirty(nameof(entity.AdminNote));
            await _unitOfWork.SaveChangesAsync();

            return Response<FeedbackDto>.Success(
                AutoMapperGeneric.Map<Feedback, FeedbackDto>(entity), StatusCode.Ok.ToDescription());
        }

        /// <summary>Số góp ý trạng thái "Mới" — badge trên nav admin.</summary>
        public async Task<Response<int>> GetNewCountAsync()
        {
            var count = await _repos.TableNoTracking.CountAsync(f => f.Status == FeedbackStatusEnums.Moi);
            return Response<int>.Success(count, StatusCode.Ok.ToDescription());
        }

        /// <summary>Email founder khi có góp ý mới — lỗi chỉ log, không chặn việc gửi góp ý.</summary>
        private async Task TryNotifyFounderAsync(Feedback fb)
        {
            try
            {
                var founderEmail = _config["Founder:Email"];
                if (string.IsNullOrWhiteSpace(founderEmail)) return;

                var tutor = await _userRepos.TableNoTracking
                    .Where(u => u.Id == fb.IdTutor)
                    .Select(u => u.DisplayName)
                    .FirstOrDefaultAsync() ?? "Tutor";

                var typeLabel = fb.Type.ToDescription();
                var stars = fb.Rating.HasValue ? $" · {fb.Rating}⭐" : "";
                var html = $@"
<div style='font-family:Inter,sans-serif;max-width:520px;margin:0 auto;padding:28px;background:#F7F8FC;color:#1A1F36'>
  <h2 style='color:#5B5FCF;margin:0 0 12px'>💬 Góp ý mới từ {System.Net.WebUtility.HtmlEncode(tutor)}</h2>
  <p style='margin:0 0 4px'><strong>[{typeLabel}{stars}]</strong> {System.Net.WebUtility.HtmlEncode(fb.Title)}</p>
  <p style='white-space:pre-line;color:#4A5568'>{System.Net.WebUtility.HtmlEncode(fb.Content)}</p>
  <hr style='border:none;border-top:1px solid #E3E6F0;margin:16px 0'/>
  <p style='color:#8A91A8;font-size:12px;margin:0'>Vào EduTrack Admin → Góp ý để phản hồi.</p>
</div>";
                await _emailService.SendAsync(founderEmail, $"[EduTrack] Góp ý mới: {fb.Title}", html, fb.Content);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không gửi được email báo founder về góp ý {Id}", fb.Id);
            }
        }
    }
}
