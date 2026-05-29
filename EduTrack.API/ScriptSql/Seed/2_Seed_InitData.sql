-- ============================================================
-- EduTrack — Seed dữ liệu khởi tạo
-- Chạy SAU khi `dotnet ef database update` đã apply migrations.
-- Idempotent: chạy lại 2-3 lần không tạo duplicate.
-- ============================================================

SET NOCOUNT ON;
DECLARE @now DATETIME = GETUTCDATE();
DECLARE @empty UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';

-- ─── 1. Roles ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'SUPPER_ADMIN' AND IsDeleted = 0)
BEGIN
    INSERT INTO Roles (Id, Code, Name, [Order], IsDeleted, DateCreated, CreatedUserId)
    VALUES (NEWID(), 'SUPPER_ADMIN', N'Super Admin', 1, 0, @now, @empty);
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'ADMIN' AND IsDeleted = 0)
BEGIN
    INSERT INTO Roles (Id, Code, Name, [Order], IsDeleted, DateCreated, CreatedUserId)
    VALUES (NEWID(), 'ADMIN', N'Quản trị viên', 2, 0, @now, @empty);
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'TUTOR' AND IsDeleted = 0)
BEGIN
    INSERT INTO Roles (Id, Code, Name, [Order], IsDeleted, DateCreated, CreatedUserId)
    VALUES (NEWID(), 'TUTOR', N'Gia sư', 3, 0, @now, @empty);
END

-- ─── 2. Link user "tutor1" vào role TUTOR ─────────────────────
-- (Tạo user "tutor1" qua POST /api/auth/register TRƯỚC khi chạy đoạn dưới)
DECLARE @idUserTutor1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE UserName = 'tutor1' AND IsDeleted = 0);
DECLARE @idRoleTutor UNIQUEIDENTIFIER  = (SELECT TOP 1 Id FROM Roles WHERE Code = 'TUTOR' AND IsDeleted = 0);

IF @idUserTutor1 IS NOT NULL AND @idRoleTutor IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE IdUser = @idUserTutor1 AND IdRole = @idRoleTutor AND IsDeleted = 0)
BEGIN
    INSERT INTO UserRoles (Id, IdUser, IdRole, IsDeleted, DateCreated, CreatedUserId)
    VALUES (NEWID(), @idUserTutor1, @idRoleTutor, 0, @now, @empty);

    PRINT N'✅ Đã link user tutor1 vào role TUTOR';
END
ELSE IF @idUserTutor1 IS NULL
    PRINT N'⚠️ Chưa có user tutor1 — chạy POST /api/auth/register trước, rồi re-run script.';
ELSE
    PRINT N'ℹ️ User tutor1 đã ở role TUTOR rồi (skip).';

-- ─── 3. (Tuỳ chọn) Seed 1 Page menu để sidebar có gì hiển thị ───
IF NOT EXISTS (SELECT 1 FROM Pages WHERE Name = N'Lịch dạy' AND IsDeleted = 0)
BEGIN
    INSERT INTO Pages (Id, Name, Url, Icon, IsActive, IsTab, IsHomePage, IdParent, PermissionCode, [Order], IsDeleted, DateCreated, CreatedUserId)
    VALUES
      (NEWID(), N'Dashboard',  'dashboard', 'dashboard', 1, 0, 1, NULL, NULL, 1, 0, @now, @empty),
      (NEWID(), N'Học sinh',   'student',   'solution',  1, 0, 0, NULL, NULL, 2, 0, @now, @empty),
      (NEWID(), N'Lịch dạy',   'lesson',    'calendar',  1, 0, 0, NULL, NULL, 3, 0, @now, @empty),
      (NEWID(), N'Hộp nhắc',   'inbox',     'bell',      1, 0, 0, NULL, NULL, 4, 0, @now, @empty);
END

PRINT N'=== Seed hoàn tất ===';
SELECT 'Roles' AS Bang, COUNT(*) AS N FROM Roles WHERE IsDeleted = 0
UNION ALL SELECT 'Users', COUNT(*) FROM Users WHERE IsDeleted = 0
UNION ALL SELECT 'UserRoles', COUNT(*) FROM UserRoles WHERE IsDeleted = 0
UNION ALL SELECT 'Pages', COUNT(*) FROM Pages WHERE IsDeleted = 0;
