-- ============================================================
-- EduTrack — Tạo Database
-- Chạy script này ĐẦU TIÊN (trên `master` database).
-- Sau đó USE EduTrack rồi chạy 1_InitDb.sql + 2_Seed_InitData.sql
-- ============================================================

USE master;
GO

IF DB_ID(N'EduTrack') IS NULL
BEGIN
    CREATE DATABASE EduTrack
    COLLATE Vietnamese_CI_AS;     -- hỗ trợ tiếng Việt có dấu, không phân biệt hoa-thường
    PRINT N'✅ Đã tạo database EduTrack';
END
ELSE
    PRINT N'ℹ️ Database EduTrack đã tồn tại — skip.';
GO

-- Recovery model SIMPLE cho dev (không cần backup log)
ALTER DATABASE EduTrack SET RECOVERY SIMPLE;
GO

USE EduTrack;
GO

PRINT N'➡️ Đang ở database EduTrack — tiếp theo chạy 1_InitDb.sql';
