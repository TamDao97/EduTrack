using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using System.Threading.Tasks;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IPageService : IBaseService<Page, PageDto>
    {
        //Task<Response<PagingData<List<UserDto>>>> GetByFilterAsync(UserGridFilter gridDto);
        //Task<Response<UserDto>> GetByIdAsync(Guid id);
        //Task<Response<bool>> DeleteAsync(Guid id);
        Task<Response<List<PageTreeNode>>> GetPageTreeAsync();
        Task<Response<List<PageTreeNode>>> GetPageTreeByUserLoginAsync();
    }

    public class PageService : BaseService<Page, PageDto>, IPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<Page> _pageRepos;
        //private readonly TD.Lib.Repository.ITDRepository<Permission> _permissionRepos;
        //private readonly TD.Lib.Repository.ITDRepository<UserRole> _userRoleRepos;
        //private readonly TD.Lib.Repository.ITDRepository<Role> _roleRepos;
        //private readonly TD.Lib.Repository.ITDRepository<RolePermission> _rolePermissionRepos;
        #endregion

        #region services
        private readonly IUserContextService _userContextService;
        #endregion

        public PageService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings
            , IUserContextService userContextService) : base(unitOfWork)
        {
            _pageRepos = unitOfWork.GetRepository<Page>();
            //_userRoleRepos = unitOfWork.GetRepository<UserRole>();
            //_roleRepos = unitOfWork.GetRepository<Role>();
            //_permissionRepos = unitOfWork.GetRepository<Permission>();
            //_rolePermissionRepos = unitOfWork.GetRepository<RolePermission>();
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _appSettings = appSettings.Value;
            _userContextService = userContextService;
        }

        public override async Task<Response<PageDto>> CreateAsync(Page entity)
        {
            return await base.CreateAsync(entity);
        }

        public override async Task<Response<PageDto>> UpdateAsync(Page entity)
        {
            //case: chọn page cha chính là page con của nó -> chặn
            if (_pageRepos.TableNoTracking.Any(r => r.IdParent == entity.Id && r.Id == entity.IdParent))
                return Response<PageDto>.Error(StatusCode.InternalServerError, $"Trang cha không hợp lệ vì nó chình là trang con của {entity.Name}");

            entity.MarkDirty(nameof(entity.Name));
            entity.MarkDirty(nameof(entity.Url));
            entity.MarkDirty(nameof(entity.Icon));
            entity.MarkDirty(nameof(entity.IsActive));
            entity.MarkDirty(nameof(entity.IsTab));
            entity.MarkDirty(nameof(entity.IsHomePage));
            entity.MarkDirty(nameof(entity.IdParent));
            entity.MarkDirty(nameof(entity.PermissionCode));
            entity.MarkDirty(nameof(entity.Order));
            return await base.UpdateAsync(entity);
        }

        public async Task<Response<List<PageTreeNode>>> GetPageTreeAsync()
        {
            var datas = await BuildPageTree();
            return Response<List<PageTreeNode>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<PageTreeNode>>> GetPageTreeByUserLoginAsync()
        {
            var datas = await BuildPageTreeByUserLogin();
            // EduTrack dùng menu phẳng — KHÔNG filter leaf root như OrderDebt.
            // (Trước đây có filter `Where(Children.Count > 0)` cắt sạch menu một cấp.)
            return Response<List<PageTreeNode>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        #region private func
        private async Task<List<PageTreeNode>> BuildPageTree()
        {
            List<Page> flatPages = _pageRepos.TableNoTracking.OrderBy(r => r.Order).ToList();

            var lookup = flatPages.ToLookup(r => r.IdParent);

            List<PageTreeNode> BuildTree(Guid? idParent)
            {
                return lookup[idParent]
                    .Select(r => new PageTreeNode
                    {
                        Key = r.Id,
                        Title = r.Name,
                        Icon = r.Icon,
                        Url = r.Url,
                        IsActive = r.IsActive,
                        IsHomePage = r.IsHomePage,
                        IsTab = r.IsTab,
                        Order = r.Order,
                        PermissionCode = r.PermissionCode,
                        Children = BuildTree(r.Id)
                    })
                    .ToList();
            }

            return BuildTree(null); // Start từ root (ParentId == null)
        }

        private async Task<List<PageTreeNode>> BuildPageTreeByUserLogin()
        {
            var userLogin = await _userContextService.GetCurrentUserAsync();
            // Super xem tất cả; user thường xem các page có matching permission HOẶC page không có PermissionCode (public).
            // Khác bản OrderDebt: bỏ check `!IdParent.HasValue` (rò rỉ trang ADMIN cho tutor thường).
            List<Page> flatPages = await _pageRepos.TableNoTracking.Where(r => userLogin.IsSuper
                                                                || (r.PermissionCode == null || r.PermissionCode == "")
                                                                || (userLogin.Permissions != null && userLogin.Permissions.Contains(r.PermissionCode)))
                                                                .OrderBy(r => r.Order).ToListAsync();

            var lookup = flatPages.ToLookup(r => r.IdParent);

            List<PageTreeNode> BuildTree(Guid? idParent)
            {
                return lookup[idParent]
                    .Select(r => new PageTreeNode
                    {
                        Key = r.Id,
                        Title = r.Name,
                        Icon = r.Icon,
                        Url = r.Url,
                        IsActive = r.IsActive,
                        IsHomePage = r.IsHomePage,
                        IsTab = r.IsTab,
                        Order = r.Order,
                        PermissionCode = r.PermissionCode,
                        Children = BuildTree(r.Id)
                    })
                    .ToList();
            }

            var tree = BuildTree(null); // Start từ root (ParentId == null)
            return tree;
        }

        // Gán số thứ tự dạng 1, 1.1, 1.1.1
        private void AssignOrderNumber(List<PageTreeNode> nodes, string prefix = "")
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                string number = string.IsNullOrEmpty(prefix)
                    ? (i + 1).ToString()             // Cấp 1 → 1,2,3
                    : $"{prefix}.{i + 1}";           // Cấp con → 1.1, 1.2...

                nodes[i].Title = $"{number}.{nodes[i].Title}";

                if (nodes[i].Children != null && nodes[i].Children.Count > 0)
                    AssignOrderNumber(nodes[i].Children, number);
            }
        }
        #endregion
    }
}
