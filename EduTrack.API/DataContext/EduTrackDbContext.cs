using EduTrack.API.Commons;
using EduTrack.API.DataContext.Entity;
using EduTrack.API.DataContext.Entity.Core;
using EduTrack.API.DataContext.Entity.TutorDomain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using TD.Lib.Repository.Entity.Base;

namespace EduTrack.API.DataContext
{
    public class EduTrackDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public bool IsHardDeleteMode { get; set; } = false;

        public EduTrackDbContext()
        {
        }

        public EduTrackDbContext(DbContextOptions<EduTrackDbContext> options, IHttpContextAccessor httpContextAccessor)
           : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #region Core
        public DbSet<Page> Pages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<EduTrack.API.DataContext.Entity.Core.File> Files { get; set; }
        public DbSet<ConfigJson> ConfigJsons { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        #endregion

        #region Tutor domain
        public DbSet<TutorProfile> TutorProfiles { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<TuitionPeriod> TuitionPeriods { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Áp dụng global query filter để lọc các entity có IsDeleted = false
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Tạo parameter "e" đại diện cho entity trong biểu thức
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    // Truy cập thuộc tính "IsDeleted"
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")!
                        .MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(
                        propertyMethodInfo,
                        parameter,
                        Expression.Constant("IsDeleted")
                    );

                    // Biểu thức điều kiện: EF.Property<bool>(e, "IsDeleted") == false
                    var compareExpression = Expression.MakeBinary(
                        ExpressionType.Equal,
                        isDeletedProperty,
                        Expression.Constant(false)
                    );

                    // Lambda: e => EF.Property<bool>(e, "IsDeleted") == false
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // Tutor domain — index tối thiểu cho query nóng
            modelBuilder.Entity<Parent>().HasIndex(p => new { p.IdTutor, p.Phone });
            modelBuilder.Entity<Student>().HasIndex(s => new { s.IdTutor, s.Status });
            modelBuilder.Entity<Student>().HasIndex(s => s.IdParent);
            modelBuilder.Entity<Lesson>().HasIndex(l => new { l.IdTutor, l.ScheduledDate });
            modelBuilder.Entity<Lesson>().HasIndex(l => new { l.IdStudent, l.ScheduledDate });
            modelBuilder.Entity<Lesson>().HasIndex(l => l.IdTuitionPeriod);
            modelBuilder.Entity<TuitionPeriod>().HasIndex(t => new { t.IdStudent, t.PeriodYear, t.PeriodMonth }).IsUnique();
            modelBuilder.Entity<TutorProfile>().HasIndex(t => t.IdUser).IsUnique();
            modelBuilder.Entity<Notification>().HasIndex(n => new { n.IdTutor, n.Status, n.ScheduledAt });
            modelBuilder.Entity<Notification>().HasIndex(n => n.RefId);
            modelBuilder.Entity<Subscription>().HasIndex(s => s.IdTutor).IsUnique();
            modelBuilder.Entity<SubscriptionPayment>().HasIndex(p => new { p.IdTutor, p.DateCreated });
            modelBuilder.Entity<SubscriptionPayment>().Property(p => p.Amount).HasColumnType("decimal(18,0)");

            modelBuilder.Entity<PasswordResetToken>().HasIndex(t => t.Token).IsUnique();
            modelBuilder.Entity<PasswordResetToken>().HasIndex(t => new { t.IdUser, t.UsedAt });

            // Tiền tệ — VND nguyên, decimal(18,0)
            modelBuilder.Entity<Student>().Property(s => s.PerLessonRate).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<Lesson>().Property(l => l.ChargeAmount).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<TuitionPeriod>().Property(t => t.TotalAmount).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<TuitionPeriod>().Property(t => t.Adjustment).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<TuitionPeriod>().Property(t => t.FinalAmount).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<TuitionPeriod>().Property(t => t.PaidAmount).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<StudentCourse>().Property(c => c.PerLessonRate).HasColumnType("decimal(18,0)");
            modelBuilder.Entity<StudentCourse>().HasIndex(c => new { c.IdStudent, c.IsActive });
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            ApplyMarkDirtyRules();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            ApplyMarkDirtyRules();
            return await base.SaveChangesAsync(cancellationToken);
        }

        #region extension methods
        private void ApplyAuditInfo()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity);

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                var now = AppTime.VnNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                        entity.CreatedUserName = userName;
                        entity.DateCreated = now;
                        entity.ModifyUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                        entity.ModifyUserName = userName;
                        entity.DateModify = now;
                        entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entity.ModifyUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                        entity.ModifyUserName = userName;
                        entity.DateModify = now;

                        entity.MarkDirty(nameof(entity.ModifyUserId));
                        entity.MarkDirty(nameof(entity.ModifyUserName));
                        entity.MarkDirty(nameof(entity.DateModify));
                        break;

                    case EntityState.Deleted:
                        if (!IsHardDeleteMode)
                        {
                            // Soft delete
                            entry.State = EntityState.Modified;
                            entity.IsDeleted = true;
                            entity.DeletedUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                            entity.DeletedUserName = userName;
                            entity.DateDeleted = now;

                            entity.MarkDirty(nameof(entity.IsDeleted));
                            entity.MarkDirty(nameof(entity.DeletedUserId));
                            entity.MarkDirty(nameof(entity.DeletedUserName));
                            entity.MarkDirty(nameof(entity.DateDeleted));
                        }
                        break;
                }
            }
        }

        private void ApplyMarkDirtyRules()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified && entry.Entity is ITrackDirty trackedEntity)
                {
                    foreach (var prop in entry.Properties)
                    {
                        // Nếu property không nằm trong danh sách MarkDirty => không update
                        if (!trackedEntity.IsDirty(prop.Metadata.Name))
                        {
                            prop.IsModified = false;
                        }
                    }

                    // Nếu sau khi lọc hết thì không còn field nào thay đổi, ta bỏ trạng thái Modified
                    if (!entry.Properties.Any(p => p.IsModified))
                    {
                        entry.State = EntityState.Unchanged;
                    }
                }
            }
        }
        #endregion
    }
}
