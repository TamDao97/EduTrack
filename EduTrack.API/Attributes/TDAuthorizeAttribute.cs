/*
 * created by:tamdc
 * create date: 29/9/2024
 */

/* Attribute này thực hiện authen & author trên hệ thống */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace EduTrack.API.Attributes
{
    public class TDAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public TDAuthorizeAttribute()
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra xác thực
            // Nếu bạn dùng JWT Bearer Authentication(AddAuthentication().AddJwtBearer(...)), thì:
            // Token có chữ ký sai → middleware sẽ reject, IsAuthenticated = false.
            // Token hết hạn(exp) → middleware reject, IsAuthenticated = false.
            // Token hợp lệ về mặt cấu trúc và chữ ký → IsAuthenticated = true.
            // => chỗ này đã đảm bảo token “valid” theo nghĩa: chưa hết hạn, chữ ký đúng, audience / issuer đúng(theo cấu hình).
            var isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;

            if (!isAuthenticated)
            {
                // Nếu không xác thực, trả về 401 Unauthorized
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionCode = ExtractPermissionFromFunc(context);

            // Nếu không có permissionCode, không cần kiểm tra quyền
            if (string.IsNullOrEmpty(permissionCode)) return;

            // Kiểm tra quyền theo yêu cầu
            var userHasPermission = CheckUserPermission(context, permissionCode);
            if (!userHasPermission)
            {
                // Nếu không có quyền, trả về 403 Forbidden
                context.Result = new ForbidResult();
            }
        }

        private bool CheckUserPermission(AuthorizationFilterContext context, string permissionCode)
        {
            // Lấy IServiceProvider
            var serviceProvider = context.HttpContext.RequestServices;

            // Lấy service user
            var userContextService = serviceProvider.GetService(typeof(IUserContextService)) as IUserContextService;

            //Logic check quyền
            var currentUser = userContextService?.GetCurrentUserAsync().Result;

            if (currentUser is null) return false;

            //case bỏ qua check quyền với super
            if (currentUser.IsSuper) return true;

            //case check quyền với user bình thường
            if (!currentUser.Permissions.Any(r => r == permissionCode)) return false;

            return true;
        }

        private string ExtractPermissionFromFunc(AuthorizationFilterContext context)
        {
            // Lấy ActionDescriptor để truy cập thông tin về action method hiện tại
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            //if (actionDescriptor != null) return string.Empty;

            // Lấy tên lớp chứa phương thức
            var controllerType = actionDescriptor.ControllerTypeInfo;

            //// Lấy attribute của lớp
            //var moduleAttribute = controllerType.GetCustomAttributes<TDModuleAttribute>().FirstOrDefault();

            // Lấy các attribute của phương thức
            var permissionAttribute = actionDescriptor.MethodInfo.GetCustomAttributes<TDPermissionAttribute>().FirstOrDefault();

            if (permissionAttribute is null || string.IsNullOrEmpty(permissionAttribute.PermissionCode)) //Hàm này không yêu cầu phân quyền
            {
                return string.Empty;
            }
            return $"{controllerType.Name}_{permissionAttribute.PermissionCode}";
        }
    }
}

