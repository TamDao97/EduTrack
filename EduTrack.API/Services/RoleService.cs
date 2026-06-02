using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EduTrack.API.Attributes;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto.Core;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using System.Reflection;
using System.Security;
using TD.Lib.AutoMapper;
using TD.Lib.Common;
using TD.Lib.Helper;
using TD.Lib.Repository;

namespace EduTrack.API.Services
{
    public interface IRoleService : IBaseService<Role, RoleDto>
    {
        #region GET
        Task<Response<PagingData<List<RoleDto>>>> GetByFilterAsync(RoleGridFilter gridDto);
        #endregion

        #region Phân quyền
        Task<Response> ScanPermissionAsync();
        Task<Response<List<PermissionGroupByModuleDto>>> GetPermissionGroupByModuleAsync();
        Task<Response<List<PermissionGroupByModuleDto>>> GetPermissionByRoleAsync(Guid idRole);
        Task<Response<int>> AddPermissionByRoleAsync(PermissionGroupByRoleCreateReqDto dtoReq);
        #endregion
    }

    public class RoleService : BaseService<Role, RoleDto>, IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<Role> _roleRepos;
        private readonly TD.Lib.Repository.ITDRepository<Permission> _permissionRepos;
        private readonly TD.Lib.Repository.ITDRepository<RolePermission> _rolePermissionRepos;
        #endregion

        public RoleService(
            IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings) : base(unitOfWork)
        {
            _roleRepos = unitOfWork.GetRepository<Role>();
            _permissionRepos = unitOfWork.GetRepository<Permission>();
            _rolePermissionRepos = unitOfWork.GetRepository<RolePermission>();
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        public override async Task<Response<RoleDto>> CreateAsync(Role entity)
        {
            string errorMess = "";
            if (IsDuplicated(ref errorMess, nameof(entity.Code), entity.Code))
                return Response<RoleDto>.Error(StatusCode.InternalServerError, string.Format($"{entity.Code} đã tồn tại trên hệ thống!"));

            if (IsDuplicated(ref errorMess, nameof(entity.Name), entity.Name))
                return Response<RoleDto>.Error(StatusCode.InternalServerError, string.Format($"{entity.Name} đã tồn tại trên hệ thống!"));

            return await base.CreateAsync(entity);
        }

        public override async Task<Response<RoleDto>> UpdateAsync(Role entity)
        {
            string errorMess = "";
            if (IsDuplicated(ref errorMess, nameof(entity.Code), entity.Code, entity.Id))
                return Response<RoleDto>.Error(StatusCode.InternalServerError, string.Format($"{entity.Code} đã tồn tại trên hệ thống!"));

            if (IsDuplicated(ref errorMess, nameof(entity.Name), entity.Name, entity.Id))
                return Response<RoleDto>.Error(StatusCode.InternalServerError, string.Format($"{entity.Name} đã tồn tại trên hệ thống!"));

            entity.MarkDirty(nameof(entity.Code));
            entity.MarkDirty(nameof(entity.Name));
            entity.MarkDirty(nameof(entity.Description));
            entity.MarkDirty(nameof(entity.Order));

            return await base.UpdateAsync(entity);
        }

        public override async Task<Response<RoleDto>> DeleteAsync(Guid id, bool isActual = false)
        {
            return await base.DeleteAsync(id, isActual);
        }

        public override async Task<Response<RoleDto>> GetByIdAsync(Guid id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<Response<PagingData<List<RoleDto>>>> GetByFilterAsync(RoleGridFilter gridDto)
        {
            var query = _roleRepos.TableNoTracking.OrderByDescending(r => r.DateModify).AsQueryable();

            if (!string.IsNullOrEmpty(gridDto.Keyword))
            {
                query = query.Where(r => r.Code.Trim().ToLower().Contains(gridDto.Keyword.Trim().ToLower())
                              || r.Code.Trim().ToLower().Contains(gridDto.Keyword.Trim().ToLower()));
            }

            int totalItems = query.Select(r => r.Id).Count();
            var lstItemsPagging = query.Skip((gridDto.PageNumber - 1) * gridDto.PageSize)
                                        .Take(gridDto.PageSize)
                                        .Select(r => AutoMapperGeneric.Map<Role, RoleDto>(r))
                                        .ToList();

            var item = PagingData<List<RoleDto>>.Create(lstItemsPagging, gridDto.PageNumber, totalItems / gridDto.PageSize, totalItems);
            return Response<PagingData<List<RoleDto>>>.Success(item, StatusCode.Ok.ToDescription());
        }

        public async Task<Response> ScanPermissionAsync()
        {
            //Get db
            var lstRolesDb = _roleRepos.TableNoTracking.ToList();
            var lstPermissionDb = _permissionRepos.TableNoTracking.ToList();
            var lstRolePermissionsDb = _rolePermissionRepos.TableNoTracking.ToList();

            // Lấy tất cả các loại (types) trong assembly hiện tại
            var assembly = Assembly.GetExecutingAssembly();
            var controllerTypes = assembly.GetTypes()
                                          .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                                          .ToList();

            var lstPermissionAdd = new List<Permission>();
            var lstPermissionUpdate = new List<Permission>();
            var lstRolePermissionAdd = new List<RolePermission>();

            Permission permission = null;
            foreach (var controller in controllerTypes)
            {
                Console.WriteLine($"Controller: {controller.Name}");

                // Lấy các attributes của controller
                var moduleAttribute = controller.GetCustomAttributes(true).FirstOrDefault(r => r.GetType().Name == nameof(TDModuleAttribute)) as TDModuleAttribute;

                if (moduleAttribute is null) continue;

                Console.WriteLine($"  [Module Attribute] {moduleAttribute.GetType().Name}");

                // Lấy các methods trong controller
                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    Console.WriteLine($"Method: {method.Name}");

                    // Lấy các attributes của method
                    var permissionAttribute = method.GetCustomAttributes(true).FirstOrDefault(r => r.GetType().Name == nameof(TDPermissionAttribute)) as TDPermissionAttribute;

                    if (permissionAttribute is null) continue;

                    Console.WriteLine($"[Method Attribute] {permissionAttribute.GetType().Name}");

                    permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        ModuleCode = controller.Name,
                        ModuleDescription = moduleAttribute.Description,
                        ModuleOrder = moduleAttribute.Order,
                        PermissionCode = $"{controller.Name}_{permissionAttribute.PermissionCode}",
                        Description = permissionAttribute.Description,
                        RoleCodes = string.Join(";", permissionAttribute.RoleCodes ?? string.Empty),
                    };
                    lstPermissionAdd.Add(permission);

                    var lstRoleCode = permissionAttribute.RoleCodes?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

                    Role role = null;
                    foreach (var roleCode in lstRoleCode)
                    {
                        role = lstRolesDb.FirstOrDefault(r => r.Code.Trim().ToUpper() == roleCode.Trim().ToUpper());

                        if (role is null) continue;

                        RolePermission rolePermission = new RolePermission
                        {
                            IdRole = role.Id,
                            IdPermission = permission.Id
                        };
                        lstRolePermissionAdd.Add(rolePermission);
                    }
                }
            }

            _unitOfWork.Context.IsHardDeleteMode = true;
            await _permissionRepos.DeleteMultiAsync(lstPermissionDb);
            await _rolePermissionRepos.DeleteMultiAsync(lstRolePermissionsDb);

            await _permissionRepos.CreateMultiAsync(lstPermissionAdd);
            await _rolePermissionRepos.CreateMultiAsync(lstRolePermissionAdd);
            await _unitOfWork.SaveChangesAsync();
            return Response.Success(StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<PermissionGroupByModuleDto>>> GetPermissionGroupByModuleAsync()
        {
            var datas = (from p in _permissionRepos.TableNoTracking
                         group p by new { p.ModuleCode, p.ModuleDescription, p.ModuleOrder } into gr
                         select new PermissionGroupByModuleDto
                         {
                             ModuleCode = gr.Key.ModuleCode,
                             ModuleDescription = gr.Key.ModuleDescription,
                             ModuleOrder = gr.Key.ModuleOrder,
                             LstPermissions = gr.Select(r => AutoMapperGeneric.Map<Permission, PermissionViewDto>(r)).ToList()
                         }).ToList();
            return Response<List<PermissionGroupByModuleDto>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<List<PermissionGroupByModuleDto>>> GetPermissionByRoleAsync(Guid idRole)
        {
            var datas = (await GetPermissionGroupByModuleAsync())?.Data;

            if (idRole == Guid.Empty) return Response<List<PermissionGroupByModuleDto>>.Success(datas, StatusCode.Ok.ToDescription());

            var lstIdPermissionForRole = _rolePermissionRepos.TableNoTracking.Where(r => r.IdRole == idRole).Select(r => r.IdPermission).ToList();

            foreach (var module in datas)
            {
                foreach (var permission in module.LstPermissions)
                {
                    permission.IsChecked = lstIdPermissionForRole.Any(r => r == permission.Id);
                }
            }
            return Response<List<PermissionGroupByModuleDto>>.Success(datas, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<int>> AddPermissionByRoleAsync(PermissionGroupByRoleCreateReqDto dtoReq)
        {
            List<Permission> lstPermission = _permissionRepos.TableNoTracking.ToList();
            List<RolePermission> lstRolePermission = new List<RolePermission>();

            RolePermission rolePermission = null;
            foreach (var permissionCode in dtoReq.LstPermissions)
            {
                var permission = lstPermission.FirstOrDefault(r => r.PermissionCode == permissionCode);
                if (permission is null) continue;

                rolePermission = new RolePermission
                {
                    IdRole = dtoReq.IdRole,
                    IdPermission = permission.Id
                };
                lstRolePermission.Add(rolePermission);
            }

            var lstDataOld = _rolePermissionRepos.Table.Where(r => r.IdRole == dtoReq.IdRole).AsEnumerable();
            _unitOfWork.Context.IsHardDeleteMode = true;
            await _rolePermissionRepos.DeleteMultiAsync(lstDataOld);
            await _rolePermissionRepos.CreateMultiAsync(lstRolePermission);
            int rs = await _unitOfWork.SaveChangesAsync();
            return Response<int>.Success(rs, StatusCode.Ok.ToDescription());
        }
    }
}
