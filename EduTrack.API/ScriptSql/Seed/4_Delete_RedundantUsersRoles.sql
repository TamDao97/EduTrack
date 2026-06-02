-- ============================================================
-- EduTrack — Xoá HẲN tài khoản & role thừa (rác scaffold OrderDebt cũ)
-- ------------------------------------------------------------
-- VÌ SAO: DB còn sót dữ liệu demo từ template OrderDebt — 3 role
--   nghiệp vụ không liên quan gia sư (mua hàng / kế toán / nhân viên)
--   + 6 tài khoản gắn các role đó. Chúng hiện ra ở /admin/users và
--   /admin/roles → "thừa, không chuẩn".
--
-- LÀM GÌ: HARD-DELETE (xoá vĩnh viễn, KHÔNG hồi lại được):
--   • 3 role: PHU_TRACH_MUA_HANG, NHAN_VIEN, KE_TOAN
--   • 6 user scaffold gắn các role đó
--   • mọi bản ghi UserRoles liên quan
--
-- GIỮ LẠI: 3 role core (SUPPER_ADMIN, ADMIN, TUTOR) + các tài khoản:
--   founder@edutrack.vn, adminsp@gmail.com, tamdc8897@gmail.com,
--   huongnt@gmail.com, comai@test.com.
--
-- ⚠️ KHÔNG hồi lại được. Nên BACKUP DB trước khi chạy.
--   Bọc trong transaction: chạy preview, kiểm tra, rồi mới COMMIT.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;   -- lỗi giữa chừng → rollback toàn bộ

-- ─── Danh sách mục tiêu (whitelist tường minh, không quét mờ) ──
DECLARE @delUsers TABLE (UserName NVARCHAR(256) PRIMARY KEY);
INSERT INTO @delUsers (UserName) VALUES
    ('huyenpham@gmail.com'),
    ('nhiltl@gmail.com'),
    ('quangnv@gmail.com'),
    ('ducnv@gmail.com'),
    ('bachluong@gmail.com'),
    ('ketoan@gmail.com');

DECLARE @delRoles TABLE (Code NVARCHAR(100) PRIMARY KEY);
INSERT INTO @delRoles (Code) VALUES
    ('PHU_TRACH_MUA_HANG'),
    ('NHAN_VIEN'),
    ('KE_TOAN');

BEGIN TRANSACTION;

-- ─── 1. Preview ─────────────────────────────────────────────
PRINT N'--- USER sẽ bị xoá vĩnh viễn: ---';
SELECT UserName, DisplayName, IsSuper FROM Users
WHERE UserName IN (SELECT UserName FROM @delUsers);

PRINT N'--- ROLE sẽ bị xoá vĩnh viễn: ---';
SELECT Code, Name FROM Roles
WHERE Code IN (SELECT Code FROM @delRoles);

-- ─── 2. Xoá UserRoles liên quan (user mục tiêu HOẶC role mục tiêu) ──
DELETE ur FROM UserRoles ur
    JOIN Users u ON u.Id = ur.IdUser
WHERE u.UserName IN (SELECT UserName FROM @delUsers);

DELETE ur FROM UserRoles ur
    JOIN Roles r ON r.Id = ur.IdRole
WHERE r.Code IN (SELECT Code FROM @delRoles);
PRINT N'✅ Đã xoá ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' bản ghi UserRoles (theo role thừa).';

-- ─── 3. Xoá Users thừa ──────────────────────────────────────
DELETE FROM Users WHERE UserName IN (SELECT UserName FROM @delUsers);
PRINT N'✅ Đã xoá ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' user thừa.';

-- ─── 4. Xoá Roles thừa ──────────────────────────────────────
DELETE FROM Roles WHERE Code IN (SELECT Code FROM @delRoles);
PRINT N'✅ Đã xoá ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' role thừa.';

-- ─── 5. Kết quả còn lại ─────────────────────────────────────
PRINT N'--- ROLE còn lại: ---';
SELECT Code, Name FROM Roles WHERE IsDeleted = 0 ORDER BY [Order];

PRINT N'--- USER còn lại: ---';
SELECT UserName, DisplayName, IsSuper FROM Users WHERE IsDeleted = 0 ORDER BY DateCreated;

-- ⚠️ Kiểm tra preview/kết quả ở trên. ĐÚNG → đổi ROLLBACK thành COMMIT rồi chạy lại.
-- (Để mặc định ROLLBACK cho an toàn, tránh lỡ tay chạy nguyên file.)
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;
