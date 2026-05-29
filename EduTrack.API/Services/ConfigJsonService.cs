using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using EduTrack.API.Commons;
using EduTrack.API.DataContext.Dto;
using EduTrack.API.DataContext.Entity;
using EduTrack.API.Services.Base;
using EduTrack.API.UnitOfWork;
using TD.Lib.Common;
using TD.Lib.Helper;

namespace EduTrack.API.Services
{
    public interface IConfigJsonService : IBaseService<ConfigJson, ConfigJsonDto>
    {
        #region config for org
        public Task<Response<JsonOrgObjectDto>> GetOrgConfigAsync();
        public Task<Response<JsonOrgObjectDto>> SaveOrgConfigAsync(JsonOrgObjectDto dtoReq);
        #endregion
    }

    public class ConfigJsonService : BaseService<ConfigJson, ConfigJsonDto>, IConfigJsonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        #region repos
        private readonly TD.Lib.Repository.ITDRepository<ConfigJson> _configJsonRepos;
        #endregion

        public ConfigJsonService(IUnitOfWork unitOfWork
            , IConfiguration configuration
            , IOptions<AppSettings> appSettings
            ) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _appSettings = appSettings.Value;
            _configJsonRepos = _unitOfWork.GetRepository<ConfigJson>();
        }

        #region config for org
        public async Task<Response<JsonOrgObjectDto>> GetOrgConfigAsync()
        {
            var item = await _configJsonRepos.TableNoTracking.FirstOrDefaultAsync();
            if (item is null) return Response<JsonOrgObjectDto>.Success(new JsonOrgObjectDto(), StatusCode.Ok.ToDescription());
            var objResult = JsonConvert.DeserializeObject<JsonOrgObjectDto>(item.JsonOrg ?? string.Empty) ?? new JsonOrgObjectDto();
            return Response<JsonOrgObjectDto>.Success(objResult, StatusCode.Ok.ToDescription());
        }

        public async Task<Response<JsonOrgObjectDto>> SaveOrgConfigAsync(JsonOrgObjectDto dtoReq)
        {
            var item = await _configJsonRepos.Table.FirstOrDefaultAsync();

            if (item is null)
            {
                item = new ConfigJson
                {
                    JsonOrg = JsonConvert.SerializeObject(dtoReq),
                    JsonSupperAdmin = string.Empty
                };
                await _configJsonRepos.CreateAsync(item);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                item.JsonOrg = JsonConvert.SerializeObject(dtoReq);
                item.MarkDirty(nameof(item.JsonOrg));
                await _configJsonRepos.UpdateAsync(item);
                await _unitOfWork.SaveChangesAsync();
            }
            return Response<JsonOrgObjectDto>.Success(dtoReq, StatusCode.Ok.ToDescription());
        }
        #endregion
    }
}