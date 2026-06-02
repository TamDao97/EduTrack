-- ============================================================
-- EduTrack — Dọn menu sidebar thừa (Pages rác từ scaffold cũ)
-- ------------------------------------------------------------
-- VÌ SAO: sidebar gia sư render MỌI Page có PermissionCode = NULL
--   (xem PageService.BuildPageTreeByUserLogin). Bảng Pages còn sót
--   các row menu cũ từ scaffold gốc (user/role/page/config, page test…)
--   nên chúng vẫn lọt vào sidebar → "menu thừa, không chuẩn".
--
-- LÀM GÌ: soft-delete (IsDeleted = 1) mọi Page KHÔNG thuộc đúng 8 url
--   chuẩn của workspace gia sư. Các trang quản trị nền tảng (/admin/*)
--   KHÔNG nằm trong bảng Pages — chúng hardcode ở FE + gác SuperGuard,
--   nên bất kỳ row nào ngoài 8 url dưới đây đều là rác.
--
-- AN TOÀN: chỉ soft-delete (đặt IsDeleted=1), có thể khôi phục bằng
--   cách set lại IsDeleted=0. Idempotent: chạy lại nhiều lần vô hại.
-- ============================================================

SET NOCOUNT ON;
DECLARE @now DATETIME = GETUTCDATE();
DECLARE @empty UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';

-- 8 url chuẩn — phải khớp với block "3. Sidebar menu tree" ở 2_Seed_InitData.sql
DECLARE @keep TABLE (Url NVARCHAR(256) PRIMARY KEY);
INSERT INTO @keep (Url) VALUES
    ('dashboard'), ('student'), ('lesson'), ('inbox'),
    ('tuition'),   ('report'),  ('billing'), ('settings');

-- ─── 1. Preview: liệt kê các menu sẽ bị dọn ──────────────────
PRINT N'--- Menu THỪA sẽ bị soft-delete: ---';
SELECT Name, Url, PermissionCode, [Order]
FROM Pages
WHERE IsDeleted = 0
  AND (Url IS NULL OR Url NOT IN (SELECT Url FROM @keep))
ORDER BY [Order];

-- ─── 2. Soft-delete các menu thừa ───────────────────────────
UPDATE Pages
SET IsDeleted     = 1,
    DateDeleted   = @now,
    DeletedUserId = @empty
WHERE IsDeleted = 0
  AND (Url IS NULL OR Url NOT IN (SELECT Url FROM @keep));

DECLARE @cleaned INT = @@ROWCOUNT;
PRINT N'✅ Đã dọn ' + CAST(@cleaned AS NVARCHAR(10)) + N' menu thừa.';

-- ─── 3. Kết quả: menu chuẩn còn lại trong sidebar ────────────
PRINT N'--- Menu chuẩn còn lại (gia sư sẽ thấy): ---';
SELECT Name, Url, [Order]
FROM Pages
WHERE IsDeleted = 0
ORDER BY [Order];
