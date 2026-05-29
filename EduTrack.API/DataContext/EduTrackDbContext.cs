using EduTrack.API.DataContext.Entity;
using EduTrack.API.DataContext.Entity.Core;
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
                var now = DateTime.UtcNow;

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
