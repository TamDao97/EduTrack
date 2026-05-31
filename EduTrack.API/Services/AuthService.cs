using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.UnitOfWork;
using System.Data.SqlTypes;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IAuthService
    {
        Task<Response<CurrentUser>> LoginAsync(LoginReq req);
        Task<Response<bool>> RegisterAsync(RegisterReq req);
        Task<Response<CurrentUser>> SignupTutorAsync(SignupTutorReq req);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<User> _userRepos;
        private readonly TD.Lib.Repository.ITDRepository<Role> _roleRepos;
        private readonly TD.Lib.Repository.ITDRepository<UserRole> _userRoleRepos;
        private readonly TD.Lib.Repository.ITDRepository<Permission> _permissionRepos;
        private readonly TD.Lib.Repository.ITDRepository<RolePermission> _rolePermissionRepos;
        #endregion

        public AuthService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings)
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;

            _userRepos = unitOfWork.GetRepository<User>();
            _roleRepos = unitOfWork.GetRepository<Role>();
            _userRoleRepos = unitOfWork.GetRepository<UserRole>();
            _permissionRepos = unitOfWork.GetRepository<Permission>();
            _rolePermissionRepos = unitOfWork.GetRepository<RolePermission>();
            _unitOfWork = unitOfWork;
        }

        #region Asp core identity
        public async Task<Response<CurrentUser>> LoginAsync(LoginReq req)
        {
            try
            {
                var user = await _userRepos.Table.FirstOrDefaultAsync(r => r.UserName == req.Email);

                if (user is null)
                    return Response<CurrentUser>.Error(StatusCode.NotFound, "Tài khoản không tồn tại trên hệ thống!");

                if (!Utils.VerifyPassword(user.PasswordHash, req.Password))
                    return Response<CurrentUser>.Error(StatusCode.BadRequest, "Thông tin đăng nhập không chính xác!");

                var tokens = JwtHelper.GenerateToken(user.UserName, user.Id.ToString(), _configuration);

                // Lấy thông tin quyền
                var roleName = (from ur in _userRoleRepos.TableNoTracking
                                where ur.IdUser == user.Id
                                join r in _roleRepos.TableNoTracking on ur.IdRole equals r.Id
                                select r.Name).FirstOrDefault();

                // Lấy danh sách quyền
                var permissions = await (from ur in _userRoleRepos.TableNoTracking
                                         where ur.IdUser == user.Id
                                         join rp in _rolePermissionRepos.TableNoTracking on ur.IdRole equals rp.IdRole
                                         join p in _permissionRepos.TableNoTracking on rp.IdPermission equals p.Id
                                         select p.PermissionCode).ToListAsync();

                var currentUser = new CurrentUser
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    IsSuper = user.IsSuper,
                    IsAdmin = user.IsAdmin,
                    RoleName = roleName,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Permissions = permissions
                };

                return Response<CurrentUser>.Success(currentUser, StatusCode.Ok.ToDescription());
            }
            catch (SqlNullValueException ex)
            {
                //Lỗi null value trong SQL — có thể do cột DateTime hoặc Number bị null

                return Response<CurrentUser>.Error(StatusCode.InternalServerError,
                    "Dữ liệu người dùng bị lỗi (thiếu thông tin bắt buộc). Vui lòng liên hệ quản trị viên!");
            }
            catch (Exception ex)
            {

                return Response<CurrentUser>.Error(StatusCode.InternalServerError, "Đăng nhập thất bại, vui lòng thử lại sau!");
            }
        }


        public async Task<Response<bool>> RegisterAsync(RegisterReq req)
        {
            //if (IsDuplicated(ref errorMess, nameof(req.UserName), req.UserName))
            //    return Response<bool>.Error(StatusCode.InternalServerError, errorMess);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = req.UserName,
                DisplayName = req.UserName,
                Email = req.UserName,
                PasswordHash = Utils.HashPassword(req.Password)
            };

            await _userRepos.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Response<bool>.Success(true, StatusCode.Ok.ToDescription());
        }

        /// <summary>
        /// Self-signup cho gia sư: validate input, tạo User, assign Role TUTOR, trả JWT.
        /// Role TUTOR phải tồn tại sẵn trong DB (seed qua 2_Seed_InitData.sql).
        /// </summary>
        public async Task<Response<CurrentUser>> SignupTutorAsync(SignupTutorReq req)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(req.UserName) || req.UserName.Trim().Length < 3)
                return Response<CurrentUser>.Error(StatusCode.BadRequest, "Email/Username phải có ít nhất 3 ký tự");
            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
                return Response<CurrentUser>.Error(StatusCode.BadRequest, "Mật khẩu phải có ít nhất 6 ký tự");
            if (req.Password != req.PasswordConfirm)
                return Response<CurrentUser>.Error(StatusCode.BadRequest, "Mật khẩu xác nhận không khớp");

            var userName = req.UserName.Trim();

            // Check duplicate
            var existed = await _userRepos.TableNoTracking.AnyAsync(u => u.UserName == userName);
            if (existed)
                return Response<CurrentUser>.Error(StatusCode.BadRequest, "Tài khoản đã tồn tại — vui lòng đăng nhập");

            // Tìm Role TUTOR
            var tutorRole = await _roleRepos.TableNoTracking.FirstOrDefaultAsync(r => r.Code == RoleCodes.Tutor);
            if (tutorRole == null)
                return Response<CurrentUser>.Error(StatusCode.InternalServerError,
                    "Hệ thống chưa cấu hình vai trò TUTOR — liên hệ quản trị viên");

            // Tạo user + gán role
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                DisplayName = req.DisplayName.Trim(),
                Email = userName,
                PasswordHash = Utils.HashPassword(req.Password),
            };

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                IdUser = user.Id,
                IdRole = tutorRole.Id,
            };

            try
            {
                await _userRepos.CreateAsync(user);
                await _userRoleRepos.CreateAsync(userRole);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Response<CurrentUser>.Error(StatusCode.InternalServerError,
                    "Đăng ký thất bại, vui lòng thử lại");
            }

            // Generate JWT — auto-login luôn
            var tokens = JwtHelper.GenerateToken(user.UserName, user.Id.ToString(), _configuration);
            var permissions = await (from rp in _rolePermissionRepos.TableNoTracking
                                     where rp.IdRole == tutorRole.Id
                                     join p in _permissionRepos.TableNoTracking on rp.IdPermission equals p.Id
                                     select p.PermissionCode).ToListAsync();

            var currentUser = new CurrentUser
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                IsAdmin = false,
                IsSuper = false,
                RoleName = tutorRole.Name,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                Permissions = permissions,
            };

            return Response<CurrentUser>.Success(currentUser, StatusCode.Ok.ToDescription());
        }
        #endregion
    }
}
