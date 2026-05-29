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
    public interface ITutorProfileService
    {
        Task<Response<TutorProfileDto>> GetMyProfileAsync();
        Task<Response<TutorProfileDto>> SaveMyProfileAsync(TutorProfileDto dto);
    }

    public class TutorProfileService : ITutorProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;
        private readonly ITDRepository<TutorProfile> _repos;

        public TutorProfileService(IUnitOfWork unitOfWork, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _repos = unitOfWork.GetRepository<TutorProfile>();
        }

        public async Task<Response<TutorProfileDto>> GetMyProfileAsync()
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Response<TutorProfileDto>.Error(StatusCode.Unauthorized, "Chưa đăng nhập");

            var entity = await _repos.TableNoTracking.FirstOrDefaultAsync(t => t.IdUser == cu.Id);
            if (entity == null)
            {
                // Trả về rỗng để FE biết là chưa setup
                return Response<TutorProfileDto>.Success(new TutorProfileDto { IdUser = cu.Id }, StatusCode.Ok.ToDescription());
            }
            return Response<TutorProfileDto>.Success(AutoMapperGeneric.Map<TutorProfile, TutorProfileDto>(entity), StatusCode.Ok.ToDescription());
        }

        public async Task<Response<TutorProfileDto>> SaveMyProfileAsync(TutorProfileDto dto)
        {
            var cu = await _userContext.GetCurrentUserAsync();
            if (cu == null) return Response<TutorProfileDto>.Error(StatusCode.Unauthorized, "Chưa đăng nhập");

            var entity = await _repos.Table.FirstOrDefaultAsync(t => t.IdUser == cu.Id);
            if (entity == null)
            {
                entity = new TutorProfile
                {
                    Id = Guid.NewGuid(),
                    IdUser = cu.Id,
                    BankName = dto.BankName,
                    BankAccountNumber = dto.BankAccountNumber,
                    BankAccountHolder = dto.BankAccountHolder,
                    Subjects = dto.Subjects,
                    Bio = dto.Bio,
                };
                await _repos.CreateAsync(entity);
            }
            else
            {
                entity.BankName = dto.BankName;
                entity.BankAccountNumber = dto.BankAccountNumber;
                entity.BankAccountHolder = dto.BankAccountHolder;
                entity.Subjects = dto.Subjects;
                entity.Bio = dto.Bio;

                entity.MarkDirty(nameof(entity.BankName));
                entity.MarkDirty(nameof(entity.BankAccountNumber));
                entity.MarkDirty(nameof(entity.BankAccountHolder));
                entity.MarkDirty(nameof(entity.Subjects));
                entity.MarkDirty(nameof(entity.Bio));
            }
            await _unitOfWork.SaveChangesAsync();
            return Response<TutorProfileDto>.Success(AutoMapperGeneric.Map<TutorProfile, TutorProfileDto>(entity), StatusCode.Ok.ToDescription());
        }
    }
}
