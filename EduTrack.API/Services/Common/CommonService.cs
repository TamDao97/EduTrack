using EduTrack.API.Commons;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.UnitOfWork;
using Microsoft.Extensions.Options;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services.Common
{
    public interface ICommonService
    {
        #region Dropdown
        Task<Response<List<Dropdown>>> DropdownPageAsync();
        Task<Response<List<Dropdown>>> DropdownUserAsync();
        Task<Response<List<Dropdown>>> DropdownRoleAsync();
        #endregion
    }

    public class CommonService : ICommonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<Page> _pageRepos;
        private readonly TD.Lib.Repository.ITDRepository<User> _userRepos;
        private readonly TD.Lib.Repository.ITDRepository<Role> _roleRepos;
        #endregion

        public CommonService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings)
        {
            _pageRepos = unitOfWork.GetRepository<Page>();
            _userRepos = unitOfWork.GetRepository<User>();
            _roleRepos = unitOfWork.GetRepository<Role>();
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        #region Dropdown
        public async Task<Response<List<Dropdown>>> DropdownPageAsync()
        {
            var datas = _pageRepos.TableNoTracking.Select(r => new Dropdown
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();
            return Response<List<Dropdown>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<Dropdown>>> DropdownUserAsync()
        {
            var datas = _userRepos.TableNoTracking.Select(r => new Dropdown
            {
                Value = r.Id,
                Text = $"{r.DisplayName} ({r.UserName})"
            }).ToList();
            return Response<List<Dropdown>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<Dropdown>>> DropdownRoleAsync()
        {
            var datas = _roleRepos.TableNoTracking.Select(r => new Dropdown
            {
                Value = r.Id,
                Text = r.Name
            }).ToList();
            return Response<List<Dropdown>>.Success(datas, StatusCode.Ok.ToDescription());
        }
        #endregion
    }
}
