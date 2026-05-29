using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IUserService : IBaseService<User, UserDto>
    {
        Task<Response<PagingData<List<UserDto>>>> GetByFilterAsync(UserGridFilter gridDto);
        Task<Response<UserCreateResDto>> GetByIdAsync(Guid id);
        Task<Response<UserCreateResDto>> CreateAsync(UserCreateReqDto reqDto);
        Task<Response<UserCreateResDto>> UpdateAsync(UserCreateReqDto reqDto);
        Task<Response<bool>> DeleteAsync(Guid id);
        Task<Response<bool>> SaveUserProfileAsync(UserProfileSaveReq req);
        Task<Response<bool>> ChangePasswordAsync(UserChangePasswordReq req, CurrentUser currentUser);
    }

    public class UserService : BaseService<User, UserDto>, IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<User> _userRepos;
        private readonly TD.Lib.Repository.ITDRepository<UserRole> _userRoleRepos;
        private readonly TD.Lib.Repository.ITDRepository<Role> _roleRepos;
        private readonly TD.Lib.Repository.ITDRepository<Permission> _permissionRepos;
        private readonly TD.Lib.Repository.ITDRepository<RolePermission> _rolePermissionRepos;
        #endregion

        public UserService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings) : base(unitOfWork)
        {
            _userRepos = unitOfWork.GetRepository<User>();
            _userRoleRepos = unitOfWork.GetRepository<UserRole>();
            _roleRepos = unitOfWork.GetRepository<Role>();
            _permissionRepos = unitOfWork.GetRepository<Permission>();
            _rolePermissionRepos = unitOfWork.GetRepository<RolePermission>();
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        public async Task<Response<UserCreateResDto>> CreateAsync(UserCreateReqDto reqDto)
        {
            if (_userRepos.TableNoTracking.Any(r => r.Id != reqDto.Id && r.UserName == reqDto.UserName))
            {
                return Response<UserCreateResDto>.Error(StatusCode.InternalServerError, string.Format($"{reqDto.UserName} đã tồn tại trên hệ thống!"));
            }

            //tạo user
            User user = AutoMapperGeneric.Map<UserCreateReqDto, User>(reqDto);

            user.PasswordHash = Utils.HashPassword(reqDto.Password);
            user.IsAdmin = await _roleRepos.TableNoTracking.AnyAsync(r => reqDto.LstIdRole.Contains(r.Id) && r.Code.Trim().ToUpper() == RoleCodes.Admin.Trim().ToUpper());

            //tạo quyền
            var lstUserRole = new List<UserRole>();

            UserRole userRole = null;
            foreach (var idRole in reqDto.LstIdRole)
            {
                userRole = new UserRole
                {
                    IdUser = user.Id,
                    IdRole = idRole
                };
                lstUserRole.Add(userRole);
            }

            string errorMess = "";
            bool isSuccess = false;
            var strategy = _unitOfWork.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Bắt đầu transaction bên trong strategy
                using var trans = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _userRepos.CreateAsync(user);
                    await _userRoleRepos.CreateMultiAsync(lstUserRole);
                    await _unitOfWork.SaveChangesAsync();
                    await trans.CommitAsync();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    errorMess = ex.Message;
                    throw; // Quan trọng: ném lại exception để strategy có thể retry nếu cần
                }
            });

            //response data
            UserCreateResDto dto = AutoMapperGeneric.Map<User, UserCreateResDto>(user);
            dto.LstIdRole = await _roleRepos.TableNoTracking.Where(r => reqDto.LstIdRole.Contains(r.Id)).Select(r => r.Id).ToListAsync();

            return Response<UserCreateResDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<UserCreateResDto>> UpdateAsync(UserCreateReqDto reqDto)
        {
            //Cập nhật user
            var user = await _userRepos.GetByIdAsync(reqDto.Id);

            if (user == null)
            {
                return Response<UserCreateResDto>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            }

            user.DisplayName = reqDto.DisplayName;
            user.Gender = reqDto.Gender;
            user.Email = reqDto.Email;
            user.PhoneNumber = reqDto.PhoneNumber;
            user.IsAdmin = await _roleRepos.TableNoTracking.AnyAsync(r => reqDto.LstIdRole.Contains(r.Id) && r.Code.Trim().ToUpper() == RoleCodes.Admin.Trim().ToUpper());

            user.MarkDirty(nameof(user.DisplayName));
            user.MarkDirty(nameof(user.Gender));
            user.MarkDirty(nameof(user.Email));
            user.MarkDirty(nameof(user.PhoneNumber));
            user.MarkDirty(nameof(user.IsAdmin));

            //Cập nhật quyền
            var lstUserRoleRemove = _userRoleRepos.Table.Where(r => r.IdUser == user.Id).AsEnumerable();

            List<UserRole> lstUserRole = new List<UserRole>();

            UserRole userRole = null;
            foreach (var idRole in reqDto.LstIdRole)
            {
                userRole = new UserRole
                {
                    IdUser = user.Id,
                    IdRole = idRole
                };
                lstUserRole.Add(userRole);
            }

            string errorMess = "";
            bool isSuccess = false;
            var strategy = _unitOfWork.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Bắt đầu transaction bên trong strategy
                using var trans = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _userRepos.UpdateAsync(user);
                    await _userRoleRepos.DeleteMultiAsync(lstUserRoleRemove);
                    await _userRoleRepos.CreateMultiAsync(lstUserRole);
                    await _unitOfWork.SaveChangesAsync();
                    await trans.CommitAsync();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    errorMess = ex.Message;
                    throw; // Quan trọng: ném lại exception để strategy có thể retry nếu cần
                }
            });

            //response data
            UserCreateResDto dto = AutoMapperGeneric.Map<User, UserCreateResDto>(user);
            dto.LstIdRole = await _roleRepos.TableNoTracking.Where(r => reqDto.LstIdRole.Contains(r.Id)).Select(r => r.Id).ToListAsync();

            return Response<UserCreateResDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<bool>> DeleteAsync(Guid id)
        {
            var user = await _userRepos.GetByIdAsync(id);

            if (user == null)
            {
                return Response<bool>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            }

            var lstUserRoleRemove = _userRoleRepos.Table.Where(r => r.IdUser == user.Id).AsEnumerable();

            string errorMess = "";
            bool isSuccess = false;
            var strategy = _unitOfWork.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Bắt đầu transaction bên trong strategy
                using var trans = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _userRepos.DeleteAsync(user);
                    await _userRoleRepos.DeleteMultiAsync(lstUserRoleRemove);
                    await _unitOfWork.SaveChangesAsync();
                    await trans.CommitAsync();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    errorMess = ex.Message;
                    throw; // Quan trọng: ném lại exception để strategy có thể retry nếu cần
                }
            });

            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<PagingData<List<UserDto>>>> GetByFilterAsync(UserGridFilter gridDto)
        {
            var query = _userRepos.TableNoTracking.OrderByDescending(r => r.DateModify).AsQueryable();

            if (!string.IsNullOrEmpty(gridDto.Keyword))
            {
                query = query.Where(r => r.UserName.Trim().ToLower().Contains(gridDto.Keyword.Trim().ToLower())
                              || r.DisplayName.Trim().ToLower().Contains(gridDto.Keyword.Trim().ToLower()));
            }

            int totalItems = query.Select(r => r.Id).Count();
            var lstItemsPagging = query.Skip((gridDto.PageNumber - 1) * gridDto.PageSize)
                                        .Take(gridDto.PageSize)
                                        .Select(r => AutoMapperGeneric.Map<User, UserDto>(r))
                                        .ToList();

            var item = PagingData<List<UserDto>>.Create(lstItemsPagging, gridDto.PageNumber, totalItems / gridDto.PageSize, totalItems);
            return Response<PagingData<List<UserDto>>>.Success(item, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<UserCreateResDto>> GetByIdAsync(Guid id)
        {
            var item = await _userRepos.GetByIdAsync(id);

            if (item == null)
                return Response<UserCreateResDto>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());

            UserCreateResDto dto = AutoMapperGeneric.Map<User, UserCreateResDto>(item);
            dto.LstIdRole = await _userRoleRepos.TableNoTracking.Where(r => r.IdUser == item.Id).Select(r => r.IdRole).ToListAsync();

            return Response<UserCreateResDto>.Success(dto, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<bool>> SaveUserProfileAsync(UserProfileSaveReq req)
        {
            //Cập nhật user
            var user = await _userRepos.GetByIdAsync(req.Id);
            if (user == null) return Response<bool>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());

            user.DisplayName = req.DisplayName;
            user.Gender = req.Gender;
            user.Email = req.Email;
            user.PhoneNumber = req.PhoneNumber;

            user.MarkDirty(nameof(user.DisplayName));
            user.MarkDirty(nameof(user.Gender));
            user.MarkDirty(nameof(user.Email));
            user.MarkDirty(nameof(user.PhoneNumber));

            await _userRepos.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<bool>> ChangePasswordAsync(UserChangePasswordReq req, CurrentUser currentUser)
        {
            var user = await _userRepos.GetByIdAsync(currentUser.Id);
            if (user == null) return Response<bool>.Error(StatusCode.NotFound, StatusCode.NotFound.ToDescription());
            if (req.Password != req.PasswordConfirm) return Response<bool>.Error(StatusCode.InternalServerError, string.Format("Mật khẩu xác nhận không chính xác!"));

            user.PasswordHash = Utils.HashPassword(req.Password);
            user.MarkDirty(nameof(user.PasswordHash));

            await _userRepos.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }
    }
}
