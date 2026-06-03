using EduTrack.API.Configs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OfficeOpenXml;
using TD.Lib.Config;

var builder = WebApplication.CreateBuilder(args);

// ─────────── Bind PORT từ env var (Render/Railway/Heroku) ───────────
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// ─────────── Services ───────────
builder.Services.LibRegisters(builder.Configuration);
builder.Services.DataContextRegisters(builder.Configuration);
builder.Services.DependencyInjection(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new EndpointTransformerCustom()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────── CORS — đọc origin từ Configuration ───────────
// Set env var Cors__AllowedOrigins = "http://localhost:4200,https://edutrack.vercel.app"
var corsOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.Trim())
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// EPPlus license
ExcelPackage.License.SetNonCommercialPersonal("EduTrack");

var app = builder.Build();

// ─────────── Seed nền tảng (role core + tài khoản founder IsSuper) ───────────
// Idempotent. Yêu cầu DB đã được migrate trước (xem RUN_LOCAL.md). Lỗi seed không chặn boot.
using (var scope = app.Services.CreateScope())
{
    try
    {
        await EduTrack.API.Configs.DataSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DataSeeder")
            .LogWarning(ex, "Bỏ qua seed nền tảng (DB chưa migrate?).");
    }
}

// ─────────── Pipeline ───────────
// Swagger luôn bật (cả prod) cho phép tutor xem API docs
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduTrack API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");

app.UseStaticFiles(); // wwwroot/uploads
// Behind Render/Vercel LB → KHÔNG dùng UseHttpsRedirection (proxy đã terminate SSL)
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint cho Render
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

app.Run();
