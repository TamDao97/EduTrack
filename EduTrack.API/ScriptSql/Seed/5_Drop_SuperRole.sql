-- ============================================================
-- EduTrack — Xoá role SUPPER_ADMIN (super giờ chỉ dùng cờ IsSuper)
-- ------------------------------------------------------------
-- VÌ SAO: founder/super KHÔNG còn dùng role nữa — chỉ định danh bằng cờ
--   User.IsSuper (bypass toàn bộ [TDPermission] ở TDAuthorize, dòng 67).
--   Sau khi gỡ ADMIN, đây là bước cuối: bỏ luôn role SUPPER_ADMIN để
--   bảng Roles chỉ còn role nghiệp vụ thật là TUTOR.
--
-- LÀM GÌ: HARD-DELETE role SUPPER_ADMIN + mọi RolePermission/UserRole
--   trỏ tới nó. DataSeeder/2_Seed mới KHÔNG seed lại role này.
--
-- AN TOÀN: bọc transaction, có preview. MẶC ĐỊNH ROLLBACK — chạy nguyên
--   file lần đầu chỉ để xem, KHÔNG xoá thật. Đúng rồi đổi ROLLBACK→COMMIT.
--   Idempotent: nếu role đã hết, chạy lại vô hại (xoá 0 dòng).
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;   -- lỗi giữa chừng → rollback toàn bộ

BEGIN TRANSACTION;

DECLARE @idSuper TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);
INSERT INTO @idSuper SELECT Id FROM Roles WHERE Code = 'SUPPER_ADMIN';

-- ─── 1. Preview ─────────────────────────────────────────────
PRINT N'--- Role SUPPER_ADMIN sẽ bị xoá (kèm links): ---';
SELECT Id, Code, Name FROM Roles WHERE Id IN (SELECT Id FROM @idSuper);

-- ─── 2. Xoá links rồi xoá role ──────────────────────────────
DELETE rp FROM RolePermissions rp WHERE rp.IdRole IN (SELECT Id FROM @idSuper);
PRINT N'✅ RolePermissions đã xoá: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

DELETE ur FROM UserRoles ur WHERE ur.IdRole IN (SELECT Id FROM @idSuper);
PRINT N'✅ UserRoles đã xoá: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

DELETE FROM Roles WHERE Id IN (SELECT Id FROM @idSuper);
PRINT N'✅ Role SUPPER_ADMIN đã xoá: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ─── 3. Kết quả ─────────────────────────────────────────────
PRINT N'--- Role còn lại (kỳ vọng: chỉ TUTOR): ---';
SELECT Code, Name FROM Roles WHERE IsDeleted = 0 ORDER BY [Order];

-- ⚠️ Kiểm tra preview/kết quả ở trên. ĐÚNG → đổi ROLLBACK thành COMMIT rồi chạy lại.
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;
