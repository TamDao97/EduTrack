# Chạy EduTrack ở local — hướng dẫn 7 bước

> Mục tiêu: từ source code → mở web app trên `http://localhost:4200`, login bằng tài khoản gia sư test, thấy các trang Dashboard / Học sinh / Lịch / Hộp nhắc.

## Yêu cầu môi trường

- [x] **.NET 8 SDK** — `dotnet --version` ≥ 8.0
- [x] **Node.js ≥ 18** + npm
- [x] **SQL Server** đang chạy (bạn đã có VPS: `vps.ngxhuyhoang.com:1433`)
- [x] **SSMS / Azure Data Studio** (để chạy SQL seed)
- [x] **dotnet-ef** (`dotnet tool install --global dotnet-ef --version 8.0.8`)

---

## Bước 1 — Cập nhật `Jwt:Key` thật

Mở `EduTrack.API/appsettings.json`, thay `REPLACE_ME_WITH_A_NEW_GUID_OR_LONG_RANDOM_STRING` bằng chuỗi ≥ 32 ký tự ngẫu nhiên. Ví dụ:

```json
"Jwt": {
  "Key": "edutrack-dev-c3f8a9b4d6e2f1a7b8c9d0e3f4a5b6c7-secret-2026",
  "Issuer": "https://edutrack.local",
  "Audience": "edutrack.local"
}
```

(Prod sau này phải đổi key khác, KHÔNG commit key thật vào git — nên dùng `appsettings.Production.json` hoặc env var.)

---

## Bước 2 — Tạo DB + schema (2 cách, chọn 1)

### Cách A — chạy 3 file SQL có sẵn (KHÔNG cần cài dotnet-ef)

Trên SSMS / Azure Data Studio, mở từng file theo thứ tự và F5:

```
EduTrack.API/ScriptSql/
├── Schema/
│   ├── 0_CreateDatabase.sql      ← chạy trên master, tạo DB EduTrack
│   └── 1_InitDb.sql              ← chạy trên DB EduTrack, tạo bảng (idempotent)
└── Seed/
    └── 2_Seed_InitData.sql       ← chạy bước 6
```

### Cách B — dùng EF tự apply migrations

Tạo trước database `EduTrack` trên SQL Server (`CREATE DATABASE EduTrack;`), rồi:

```powershell
cd D:\Coderkechuyen\EduTrack\EduTrack.API
dotnet ef database update
```

Output cuối phải là `Done.` Nếu lỗi connection → kiểm tra `ConnectionStrings:EduTrackDbContextConnection` trong `appsettings.json`.

---

## Bước 3 — Trust HTTPS dev cert (chỉ làm 1 lần / máy)

```powershell
dotnet dev-certs https --trust
```

Click "Yes" khi Windows hỏi. Nếu skip bước này, browser sẽ kêu "Not secure" khi gọi `https://localhost:7246`.

---

## Bước 4 — Start Backend

```powershell
cd D:\Coderkechuyen\EduTrack\EduTrack.API
dotnet run
```

Đợi thấy `Now listening on: https://localhost:7246` → mở `https://localhost:7246/swagger` để xác nhận API chạy được.

---

## Bước 5 — Tạo tài khoản gia sư test qua Swagger

Trên `https://localhost:7246/swagger`:

1. Tìm endpoint `POST /api/auth/register`
2. Click "Try it out"
3. Paste body:
   ```json
   {
     "userName": "tutor1",
     "password": "123456",
     "passwordConfirm": "123456"
   }
   ```
4. Click "Execute" → response phải là `{ "status": 200, ... }`

---

## Bước 6 — Chạy SQL seed (link user vào role TUTOR + tạo Page menu)

Mở SSMS, kết nối DB `EduTrack`, mở file:
```
D:\Coderkechuyen\EduTrack\EduTrack.API\ScriptSql\Seed\2_Seed_InitData.sql
```

Nhấn F5 để chạy. Output cuối phải có:
```
✅ Đã link user tutor1 vào role TUTOR
=== Seed hoàn tất ===
```

Script idempotent — chạy lại nhiều lần không tạo trùng.

---

## Bước 7 — Start Frontend

Mở terminal MỚI (giữ BE đang chạy ở terminal cũ):

```powershell
cd D:\Coderkechuyen\EduTrack\EduTrack.WEB
npm install --legacy-peer-deps     # nếu chưa từng install
npm start
```

Đợi `Compiled successfully.` → mở `http://localhost:4200`.

**Login:**
- Email: `tutor1`
- Password: `123456`

(Login API kiểm `r.UserName == req.Email`, nên field "Email" nhập `tutor1` cũng OK.)

---

## Đăng nhập Platform Console (founder / chủ nền tảng)

Founder là tài khoản vận hành nền tảng (`IsSuper=true`) — thấy khu `/admin/*` (doanh thu, danh sách tutor, thanh toán, quản trị hệ thống), khác hẳn workspace gia sư.

- **Tự động**: khi `dotnet run`, `DataSeeder` đọc khối `Founder` trong `appsettings.json` và tạo sẵn tài khoản nếu chưa có.
  - Email: `founder@edutrack.vn` · Password: `Founder@123` (đổi trong `appsettings.json`; prod để trống và set env var `Founder__Password`).
- Sau khi login bằng tài khoản này → tự chuyển tới `/admin/dashboard` (gia sư thường vào `/dashboard`).
- **Nâng 1 user có sẵn thành founder** (thủ công): bỏ comment block "4. Founder" trong `2_Seed_InitData.sql`, sửa `@founderEmail`, chạy lại — hoặc nhanh gọn:
  ```sql
  UPDATE Users SET IsSuper = 1 WHERE UserName = N'<email_user>' AND IsDeleted = 0;
  ```

---

## Checklist test sau khi vào app

1. [ ] **Dashboard** `/dashboard` — thấy trang chào mừng
2. [ ] **Học sinh** `/student` — thấy empty state "Chưa có học sinh nào · [+ Thêm học sinh đầu tiên]"
3. [ ] Click "+ Thêm học sinh đầu tiên" → modal bottom-sheet
4. [ ] Click nút `+` cạnh dropdown Phụ huynh → modal lồng để tạo PH mới (tên + SĐT)
5. [ ] Sau khi tạo PH + HS → quay lại list, thấy card HS với avatar + status tag
6. [ ] Click vào card → trang detail
7. [ ] **Lịch dạy** `/lesson` — header "Lịch dạy" + 7 ngày dọc + button "+ Thêm lịch"
8. [ ] Click "+ Thêm lịch" → menu "Thêm 1 buổi" / "Tạo lịch lặp lại"
9. [ ] Tạo lịch lặp lại 4 tuần, chọn T2 T4 T6 19h-20h30 → thấy 12 buổi được tạo
10. [ ] **Hộp nhắc** `/inbox` — thấy 24 nhắc (12 buổi × 2 nhắc T-18h + T-1h), sort theo `ScheduledAt`
11. [ ] Click 1 card → expand thấy nội dung text + 3 nút (Mở Zalo / Copy / Đã gửi)
12. [ ] Click "Copy text" → toast thành công + clipboard có text

Mỗi mục check OK = phần đó hoạt động đúng. Nếu mục nào fail, gửi tôi screenshot + console error.

---

## Troubleshooting nhanh

| Triệu chứng | Xử lý |
|---|---|
| `dotnet ef` not found | `dotnet tool install --global dotnet-ef --version 8.0.8` |
| SQL connection fail | Check `appsettings.json`, mở SSMS connect bằng đúng string đó xem có vào được không |
| Browser "Not secure" trên https://localhost:7246 | `dotnet dev-certs https --trust` rồi restart browser |
| FE gọi API CORS error | Đảm bảo `Program.cs` allow `http://localhost:4200` (đã có sẵn) + BE phải đang chạy |
| Login 401 | Đảm bảo password gõ đúng + đã chạy seed SQL để link role |
| Sidebar trống / không có menu | Chưa chạy seed Pages — re-run 2_Seed_InitData.sql |
| `npm install` peer dep conflict | Phải dùng `--legacy-peer-deps` (ag-grid-angular@35 cần Angular 18+) |
| Token expired sau vài phút | Đó là JWT lifetime mặc định — login lại |

---

## Lệnh thường dùng

```powershell
# BE
cd D:\Coderkechuyen\EduTrack\EduTrack.API
dotnet ef migrations add <Name> --output-dir Migrations  # tạo migration mới
dotnet ef database update                                # apply
dotnet run                                               # chạy

# FE
cd D:\Coderkechuyen\EduTrack\EduTrack.WEB
npm start                                                # ng serve
npm run build                                            # production build
```
