using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services.Common;
using EduTrack.API.Services.TutorDomain;
using EduTrack.API.UnitOfWork;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IAuthService
    {
        Task<Response<CurrentUser>> LoginAsync(LoginReq req);
        Task<Response<bool>> RegisterAsync(RegisterReq req);
        Task<Response<CurrentUser>> SignupTutorAsync(SignupTutorReq req);
        Task<Response<bool>> ForgotPasswordAsync(ForgotPasswordReq req);
        Task<Response<bool>> ResetPasswordAsync(ResetPasswordReq req);
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

        private readonly ISubscriptionService _subService;
        private readonly IEmailService _emailService;
        private readonly TD.Lib.Repository.ITDRepository<PasswordResetToken> _resetRepos;

        public AuthService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings
            , ISubscriptionService subService
            , IEmailService emailService)
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;

            _userRepos = unitOfWork.GetRepository<User>();
            _roleRepos = unitOfWork.GetRepository<Role>();
            _userRoleRepos = unitOfWork.GetRepository<UserRole>();
            _permissionRepos = unitOfWork.GetRepository<Permission>();
            _rolePermissionRepos = unitOfWork.GetRepository<RolePermission>();
            _resetRepos = unitOfWork.GetRepository<PasswordResetToken>();
            _unitOfWork = unitOfWork;
            _subService = subService;
            _emailService = emailService;
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

                // Tạo Trial Subscription 14 ngày
                await _subService.EnsureTrialAsync(user.Id);
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

        /// <summary>
        /// Sinh token reset password 1h + gửi email link `/reset-password?token=xxx`.
        /// Luôn trả Success bất kể email tồn tại hay không — không leak thông tin user.
        /// Token cũ chưa dùng của cùng user bị mark UsedAt=now để vô hiệu hoá.
        /// </summary>
        public async Task<Response<bool>> ForgotPasswordAsync(ForgotPasswordReq req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return Response<bool>.Error(StatusCode.BadRequest, "Vui lòng nhập email");

            var email = req.Email.Trim();
            var user = await _userRepos.Table.FirstOrDefaultAsync(u => u.UserName == email || u.Email == email);
            if (user == null)
            {
                // Không leak — vẫn báo success
                return Response<bool>.Success(true, "Nếu email tồn tại, link reset đã được gửi.");
            }

            // Vô hiệu hoá token cũ
            var oldTokens = await _resetRepos.Table
                .Where(t => t.IdUser == user.Id && t.UsedAt == null)
                .ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var old in oldTokens)
            {
                old.UsedAt = now;
                old.MarkDirty(nameof(old.UsedAt));
            }

            // Sinh token mới 64 ký tự url-safe
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            var token = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                IdUser = user.Id,
                Token = token,
                ExpiresAt = now.AddHours(1),
            };
            await _resetRepos.CreateAsync(resetToken);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email
            var webUrl = _configuration["App:WebUrl"] ?? "http://localhost:4200";
            var link = $"{webUrl.TrimEnd('/')}/reset-password?token={token}";
            var displayName = string.IsNullOrEmpty(user.DisplayName) ? user.UserName : user.DisplayName;
            var html = $@"
<div style='font-family:Inter,Segoe UI,sans-serif;max-width:520px;margin:0 auto;padding:32px;background:#F7F8FC;color:#1A1F36'>
  <h2 style='color:#5B5FCF;margin:0 0 12px'>🔐 Reset mật khẩu EduTrack</h2>
  <p>Xin chào {displayName},</p>
  <p>Bạn (hoặc ai đó dùng email này) vừa yêu cầu đặt lại mật khẩu. Bấm nút dưới để tạo mật khẩu mới — link có hiệu lực <strong>1 giờ</strong>.</p>
  <p style='text-align:center;margin:28px 0'>
    <a href='{link}' style='background:linear-gradient(135deg,#5B5FCF,#7C3AED);color:#fff;padding:14px 28px;border-radius:8px;text-decoration:none;font-weight:700;display:inline-block'>Đặt lại mật khẩu</a>
  </p>
  <p style='font-size:13px;color:#8A91A8'>Nếu nút không hoạt động, copy link vào trình duyệt:<br><span style='word-break:break-all;color:#4549B8'>{link}</span></p>
  <hr style='border:none;border-top:1px solid #E8EAF3;margin:24px 0'>
  <p style='font-size:12px;color:#8A91A8'>Nếu bạn không yêu cầu reset, hãy bỏ qua email này. Mật khẩu sẽ không bị thay đổi.</p>
</div>";
            await _emailService.SendAsync(user.Email ?? user.UserName!, "Reset mật khẩu EduTrack", html);

            return Response<bool>.Success(true, "Nếu email tồn tại, link reset đã được gửi.");
        }

        /// <summary>
        /// Nhận token + mật khẩu mới → verify token còn hạn + chưa dùng → đổi password user.
        /// </summary>
        public async Task<Response<bool>> ResetPasswordAsync(ResetPasswordReq req)
        {
            if (string.IsNullOrWhiteSpace(req.Token))
                return Response<bool>.Error(StatusCode.BadRequest, "Token không hợp lệ");
            if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
                return Response<bool>.Error(StatusCode.BadRequest, "Mật khẩu phải có ít nhất 6 ký tự");

            var now = DateTime.UtcNow;
            var entry = await _resetRepos.Table.FirstOrDefaultAsync(t =>
                t.Token == req.Token && t.UsedAt == null && t.ExpiresAt > now);
            if (entry == null)
                return Response<bool>.Error(StatusCode.BadRequest, "Link đã hết hạn hoặc không hợp lệ. Vui lòng tạo yêu cầu mới.");

            var user = await _userRepos.Table.FirstOrDefaultAsync(u => u.Id == entry.IdUser);
            if (user == null)
                return Response<bool>.Error(StatusCode.NotFound, "User không tồn tại");

            user.PasswordHash = Utils.HashPassword(req.NewPassword);
            user.MarkDirty(nameof(user.PasswordHash));

            entry.UsedAt = now;
            entry.MarkDirty(nameof(entry.UsedAt));

            await _unitOfWork.SaveChangesAsync();
            return Response<bool>.Success(true, "Đặt lại mật khẩu thành công. Đăng nhập với mật khẩu mới.");
        }
        #endregion
    }
}
