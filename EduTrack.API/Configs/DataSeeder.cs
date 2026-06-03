using EduTrack.API.Commons;
using EduTrack.API.DataContext;
using EduTrack.API.DataContext.Entity.Core;
using Microsoft.EntityFrameworkCore;
using TD.Lib.Helper;

namespace EduTrack.API.Configs
{
    /// <summary>
    /// Seed dữ liệu nền tảng lúc khởi động: đảm bảo role nghiệp vụ TUTOR + 1 tài khoản founder (IsSuper).
    /// Idempotent — chạy mỗi lần boot nhưng chỉ tạo phần còn thiếu, không tạo trùng.
    /// Mục đích chính: vá lỗ hổng "không user nào có IsSuper=true" → trang /admin không ai vào được.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<EduTrackDbContext>();
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<EduTrackDbContext>>();

            await EnsureRolesAsync(db);
            await EnsureFounderAsync(db, config, logger);
        }

        /// <summary>Tạo role nghiệp vụ nếu thiếu — khớp với ScriptSql/Seed/2_Seed_InitData.sql.</summary>
        private static async Task EnsureRolesAsync(EduTrackDbContext db)
        {
            // Chỉ còn 1 role nghiệp vụ: gia sư (TUTOR).
            // Founder/super KHÔNG dùng role — chỉ định danh bằng cờ User.IsSuper (bypass [TDPermission]).
            var seeds = new (string Code, string Name, int Order)[]
            {
                (RoleCodes.Tutor, "Gia sư", 1),
            };

            var existingCodes = await db.Roles.Select(r => r.Code).ToListAsync();
            var added = false;
            foreach (var (code, name, order) in seeds)
            {
                if (existingCodes.Contains(code)) continue;
                db.Roles.Add(new Role { Id = Guid.NewGuid(), Code = code, Name = name });
                added = true;
            }
            if (added) await db.SaveChangesAsync();
        }

        /// <summary>
        /// Đảm bảo tài khoản founder tồn tại. Đọc cấu hình "Founder" (Email/Password/DisplayName).
        /// Prod nên set qua env var Founder__Password thay vì commit mật khẩu thật.
        /// </summary>
        private static async Task EnsureFounderAsync(EduTrackDbContext db, IConfiguration config, ILogger logger)
        {
            var section = config.GetSection("Founder");
            var email = section.GetValue<string>("Email");
            var password = section.GetValue<string>("Password");
            var displayName = section.GetValue<string>("DisplayName") ?? "Founder";

            // Không cấu hình founder → bỏ qua (cho phép tắt seeder ở môi trường không cần).
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation("Founder seeder: chưa cấu hình Founder:Email/Password — bỏ qua.");
                return;
            }

            var founder = await db.Users.FirstOrDefaultAsync(u => u.UserName == email);
            if (founder == null)
            {
                founder = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    DisplayName = displayName,
                    PasswordHash = Utils.HashPassword(password),
                    IsSuper = true,
                };
                db.Users.Add(founder);
                await db.SaveChangesAsync();
                logger.LogInformation("Founder seeder: đã tạo tài khoản founder {Email}.", email);
            }
            else if (!founder.IsSuper)
            {
                // User cùng email đã tồn tại nhưng chưa phải founder → nâng quyền.
                founder.IsSuper = true;
                founder.MarkDirty(nameof(founder.IsSuper));
                await db.SaveChangesAsync();
                logger.LogInformation("Founder seeder: đã nâng {Email} thành founder (IsSuper=true).", email);
            }

            // Founder KHÔNG cần role — quyền truy cập đến từ cờ IsSuper (bypass mọi [TDPermission]).
        }
    }
}
