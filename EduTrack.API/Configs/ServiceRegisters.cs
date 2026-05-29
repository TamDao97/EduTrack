using EduTrack.API.DataContext;
using EduTrack.API.Services;
using EduTrack.API.Services.Common;
using EduTrack.API.Services.TutorDomain;
using EduTrack.API.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduTrack.API.Configs
{
    public static class ServiceRegister
    {
        // Add services to the container.
        public static void DataContextRegisters(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<EduTrackDbContext>(opts => opts.UseSqlServer(
                                                                config["ConnectionStrings:EduTrackDbContextConnection"],
                                                                sqlOptions => sqlOptions.EnableRetryOnFailure(
                                                                    maxRetryCount: 5,
                                                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                                                    errorNumbersToAdd: null
                                                      ))
                                                    .EnableSensitiveDataLogging() // ⚠️ chỉ bật DEV
                                                    .LogTo(Console.WriteLine, LogLevel.Information)
            );

            // Configure Authentication
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                var Key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void DependencyInjection(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork.UnitOfWork));

            #region Core services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IConfigJsonService, ConfigJsonService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<ExcelService>();
            #endregion

            #region Tutor domain
            services.AddScoped<ITutorProfileService, TutorProfileService>();
            services.AddScoped<IParentService, ParentService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ILessonService, LessonService>();
            services.AddScoped<ITuitionPeriodService, TuitionPeriodService>();
            #endregion

            services.AddHttpContextAccessor();
            services.AddSingleton<DapperContext>();
        }
    }
}
