using Microsoft.AspNetCore.Mvc;
using EduTrack.API.DataContext.Dto;

namespace EduTrack.API.Controllers.Base
{
    public abstract partial class ApiController : ControllerBase
    {
        protected CurrentUser CurrentUser { get { return GetCurrentUser(); } }

        CurrentUser? GetCurrentUser()
        {
            var service = (IUserContextService)HttpContext.RequestServices.GetServices(typeof(IUserContextService)).SingleOrDefault();
            return service?.GetCurrentUserAsync().Result;
        }
    }
}
