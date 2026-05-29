using Microsoft.EntityFrameworkCore;
using EduTrack.API.DataContext;
using EduTrack.API.DataContext.Dto;

public interface IUserContextService
{
    Task<CurrentUser> GetCurrentUserAsync();
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EduTrackDbContext _EduTrackDbContext;

    public UserContextService(EduTrackDbContext EduTrackDbContext, IHttpContextAccessor httpContextAccessor)
    {
        _EduTrackDbContext = EduTrackDbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        string? userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var user = await _EduTrackDbContext.Users.FirstOrDefaultAsync(r => r.UserName == userName);

        if (user is null) return null;

        // Lấy thông tin quyền
        var roleName = (from ur in _EduTrackDbContext.UserRoles
                        where ur.IdUser == user.Id
                        join r in _EduTrackDbContext.Roles on ur.IdRole equals r.Id
                        select r.Name).FirstOrDefault();

        //get permission
        var permissions = (from ur in _EduTrackDbContext.UserRoles
                           where ur.IdUser == user.Id
                           join rp in _EduTrackDbContext.RolePermissions on ur.IdRole equals rp.IdRole
                           join p in _EduTrackDbContext.Permissions on rp.IdPermission equals p.Id
                           select p.PermissionCode).ToList();

        var currentUser = new CurrentUser
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IsSuper = user.IsSuper,
            IsAdmin = user.IsAdmin,
            RoleName = roleName,
            Permissions = permissions
        };
        return currentUser;
    }
}
