using EduTrack.API.Configs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OfficeOpenXml;
using TD.Lib.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.LibRegisters(builder.Configuration);
builder.Services.DataContextRegisters(builder.Configuration);
builder.Services.DependencyInjection(builder.Configuration);

builder.Services.AddControllers(options =>
{
    // Thiết lập route template mặc định cho tất cả controller
    options.Conventions.Add(new RouteTokenTransformerConvention(new EndpointTransformerCustom()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                   "http://localhost:4200"
               ) // domain FE
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // nếu cần cookie/token
    });
});

// Set EPPlus 8 License
ExcelPackage.License.SetNonCommercialPersonal("EduTrack");

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger for all environments (not just development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduTrack API V1");
    c.RoutePrefix = "swagger"; // Access at /swagger/index.html
});

// Dùng CORS policy ở đây
app.UseCors("AllowFrontend");

app.UseStaticFiles(); // Để truy cập wwwroot/uploads
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
