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
            await EnsurePagesAsync(db, logger);
            await EnsureDefaultCoursesAsync(db, logger);
        }

        /// <summary>
        /// Backfill StudentCourse: HS nào CHƯA có môn nào → tạo 1 môn mặc định từ
        /// Student.Subject + PerLessonRate. Idempotent — HS đã có môn thì bỏ qua.
        /// </summary>
        private static async Task EnsureDefaultCoursesAsync(EduTrackDbContext db, ILogger logger)
        {
            var studentsNoCourse = await db.Students
                .Where(s => !db.StudentCourses.Any(c => c.IdStudent == s.Id))
                .Select(s => new { s.Id, s.IdTutor, s.Subject, s.PerLessonRate })
                .ToListAsync();

            if (studentsNoCourse.Count == 0) return;

            foreach (var s in studentsNoCourse)
            {
                db.StudentCourses.Add(new DataContext.Entity.TutorDomain.StudentCourse
                {
                    Id = Guid.NewGuid(),
                    IdTutor = s.IdTutor,
                    IdStudent = s.Id,
                    Subject = string.IsNullOrWhiteSpace(s.Subject) ? "Chung" : s.Subject.Trim(),
                    PerLessonRate = s.PerLessonRate,
                    IsActive = true,
                });
            }
            await db.SaveChangesAsync();
            logger.LogInformation("Course seeder: đã tạo môn mặc định cho {Count} HS chưa có môn.", studentsNoCourse.Count);
        }

        /// <summary>
        /// Đảm bảo các page sidebar chuẩn tồn tại (thêm page mới chỉ cần thêm dòng ở đây —
        /// boot tự insert, không phải chạy SQL tay). Chỉ insert page THIẾU theo Url, không sửa page có sẵn.
        /// </summary>
        private static async Task EnsurePagesAsync(EduTrackDbContext db, ILogger logger)
        {
            // (Name, Url, Icon, Order) — PermissionCode null = mọi user đăng nhập đều thấy.
            var seeds = new (string Name, string Url, string Icon, int Order)[]
            {
                ("Dashboard",      "dashboard", "dashboard",   1),
                ("Học sinh",       "student",   "solution",    2),
                ("Lớp học",        "classroom", "cluster",     3),
                ("Lịch dạy",       "lesson",    "calendar",    3),
                ("Hộp nhắc",       "inbox",     "bell",        4),
                ("Học phí",        "tuition",   "dollar",      5),
                ("Báo cáo",        "report",    "bar-chart",   6),
                ("Gói thanh toán", "billing",   "credit-card", 7),
                ("Cài đặt",        "settings",  "setting",     8),
                ("Góp ý",          "feedback",  "message",     9),
            };

            var existingUrls = await db.Pages.Select(p => p.Url).ToListAsync();
            var added = 0;
            foreach (var (name, url, icon, order) in seeds)
            {
                if (existingUrls.Contains(url)) continue;
                db.Pages.Add(new Page
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Url = url,
                    Icon = icon,
                    IsActive = true,
                    IsTab = false,
                    IsHomePage = url == "dashboard",
                    Order = order,
                });
                added++;
            }
            if (added > 0)
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Page seeder: đã thêm {Count} page sidebar còn thiếu.", added);
            }
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
