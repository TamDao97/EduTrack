# EduTrack — Deploy production (all-Render)

> Mục tiêu: từ `git push develop` → app live trên Internet trong ~30 phút. Cả FE + BE trên Render, 1 Blueprint. Free tier toàn bộ.

## Kiến trúc deploy

```
┌────────────────────────────────────┐
│ Render Static Site (FE — Angular)  │
│ https://edutrack-web.onrender.com   │
└─────────────┬──────────────────────┘
              │ HTTPS calls
              ▼
┌────────────────────────────────────┐
│ Render Web Service (BE — .NET 8)   │
│ https://edutrack-api.onrender.com   │  region: singapore
└─────────────┬──────────────────────┘
              │ SQL TCP 1433 (Encrypt=true)
              ▼
┌────────────────────────────────────┐
│ Your VPS (SQL Server)              │
│ vps.ngxhuyhoang.com:1433            │
└────────────────────────────────────┘
```

**Chi phí**: 0đ (Render free tier 750h/month cho web service + static site free). VPS đã có sẵn.

**Trade-off Render free**: BE sleep sau 15 phút không request → request đầu sau ngủ mất ~30-60s (wake up + `Database.Migrate()`). OK cho beta, upgrade Starter $7/mo khi cần always-on. Static site KHÔNG ngủ.

---

## Bước 1 — Push code mới nhất

```powershell
cd D:\Coderkechuyen\EduTrack
git push origin develop
```

`render.yaml` ở root định nghĩa cả 2 service (`edutrack-api` Docker + `edutrack-web` static) → Render dựng cả hai trong 1 Blueprint.

## Bước 2 — Tạo Blueprint trên Render

1. Vào https://render.com → đăng ký free (sign in with GitHub)
2. **New +** → **Blueprint** → connect repo `TamDao97/EduTrack`
3. Render đọc `render.yaml` → tạo `edutrack-api` + `edutrack-web`
4. Apply → cả 2 service vào hàng đợi build

## Bước 3 — Set env vars cho `edutrack-api`

Vào service `edutrack-api` → tab **Environment** → **Add Environment Variable**:

| Key | Value | Secret? |
|---|---|---|
| `ConnectionStrings__EduTrackDbContextConnection` | `Server=vps.ngxhuyhoang.com,1433;Database=EduTrack;User Id=YOUR_USER;Password=YOUR_PASS;Encrypt=true;TrustServerCertificate=true` | ✅ |
| `Jwt__Key` | chuỗi random ≥ 32 ký tự | ✅ |
| `Cors__AllowedOrigins` | `https://edutrack-web.onrender.com` (URL static site — điền sau bước 4 nếu Render gắn hậu tố) | – |
| `Smtp__Enabled` | `true` | – |
| `Smtp__User` | email Gmail | ✅ |
| `Smtp__Password` | Gmail App Password (16 ký tự) | ✅ |
| `Smtp__FromAddress` | trùng email Gmail (Gmail ghi đè From) | – |

> ⚠️ **Bảo mật DB**: kết nối Render→VPS đi qua Internet công cộng — BẮT BUỘC `Encrypt=true`. Không dùng `sa`; tạo SQL login riêng chỉ có quyền trên DB `EduTrack`. Firewall VPS phải mở cổng 1433 cho egress IP của Render (region Singapore) — Render free IP động, có thể phải mở rộng kèm login mạnh để bù.

`ASPNETCORE_ENVIRONMENT=Production`, `Jwt__Issuer`, `Jwt__Audience` đã khai trong `render.yaml`, không cần set tay.

## Bước 4 — Chờ build + lấy URL

1. **edutrack-api**: build Docker ~5-8 phút lần đầu. Khi log hiện `Now listening on http://+:<PORT>` → sẵn sàng.
   - Test: `https://edutrack-api.onrender.com/health` → trả `{"status":"ok",...}`
2. **edutrack-web**: build Angular (`npm run build:prod`) ~3-5 phút → serve tĩnh.
3. Copy URL thật Render gán cho cả 2 (nếu tên bị trùng global, Render thêm hậu tố `-xxxx`).

## Bước 5 — Apply DB schema (lần đầu)

VPS SQL Server đã có DB `EduTrack`. BE tự chạy `Database.Migrate()` lúc khởi động — nhưng nếu muốn chủ động:

```powershell
cd D:\Coderkechuyen\EduTrack\EduTrack.API
$env:ConnectionStrings__EduTrackDbContextConnection = "<production connection string>"
dotnet ef database update
```

## Bước 6 — Khớp URL hai chiều (chicken-egg)

1. **FE → API**: sửa `EduTrack.WEB/src/environment.production.ts` nếu URL API thật khác mặc định:
   ```typescript
   export const environment = {
       production: true,
       apiUrl: 'https://edutrack-api.onrender.com/api',   // ← URL Render thật
       fileUrl: 'https://edutrack-api.onrender.com',
   };
   ```
   Commit + push → `edutrack-web` auto-rebuild.

2. **API ← FE (CORS)**: Render → `edutrack-api` → Environment → đảm bảo `Cors__AllowedOrigins` = đúng URL `edutrack-web` (KHÔNG có dấu `/` cuối). Save → service restart.

## Bước 7 — Smoke test prod

1. Mở `https://edutrack-web.onrender.com`
2. **Đăng ký miễn phí** → tạo account `tutor1@beta.com / 123456`
3. Thấy "Đăng ký thành công" + redirect `/dashboard` → ✅
4. Test: thêm 1 HS → 1 buổi → mark "Đã dạy" → chốt kỳ → ghi nhận thu
5. Reload deep-link (vd `/lesson`) → không bị 404 (SPA rewrite hoạt động)

Nếu lỗi:
- **F12 Console** xem CORS lỗi gì
- **Render Logs** (`edutrack-api`) xem BE error / SQL connection
- **Network tab** xem request có tới đúng API URL không

## Bước 8 — Custom domain (tuỳ chọn)

Khi ra mắt với domain `edutrack.app`:

1. `edutrack-web` → Settings → Custom Domains → add `edutrack.app` → set DNS theo hướng dẫn
2. `edutrack-api` → Settings → Custom Domains → add `api.edutrack.app` → set CNAME
3. Sửa `environment.production.ts` apiUrl → `https://api.edutrack.app/api`
4. Update `Cors__AllowedOrigins` thêm `https://edutrack.app`

## Bước 9 — Onboard 5-10 tutor beta

- Nhắn gia sư quen:
  > "Em đang làm app quản lý gia sư. Bạn dùng giúp em 1 tuần feedback nhé? Link: https://edutrack-web.onrender.com — đăng ký free. Có gì khó dùng nhắn em sửa ngay."
- Track funnel: signup → thêm HS → tạo lesson → chốt kỳ
- Sau 1 tuần survey 3 câu: thích nhất / ghét nhất / sẵn sàng trả 99k/tháng nếu có tính năng X Y Z?

---

## Troubleshooting

| Triệu chứng | Fix |
|---|---|
| BE build fail "Cannot find TD.Lib" | Dockerfile context phải là root repo (`dockerContext: .` trong render.yaml — đã đúng) |
| FE build fail "ng: command not found" / peer deps | `buildCommand` đã có `npm install --legacy-peer-deps` |
| FE trắng trang / 404 khi reload deep-link | `staticPublishPath` phải là `dist/edutrack.web-prod/browser` + route rewrite `/* → /index.html` (đã set trong render.yaml) |
| CORS lỗi từ FE | `Cors__AllowedOrigins` = đúng URL static (no trailing slash) |
| 504 timeout request đầu | BE Render sleep — chấp nhận hoặc upgrade $7/mo |
| SQL connection timeout từ Render → VPS | VPS firewall phải allow egress IP Render Singapore tới 1433. Hoặc migrate sang Render PostgreSQL (cần đổi provider Npgsql + tạo lại migration) |
| SQL "certificate chain" error | Thêm `TrustServerCertificate=true` (VPS dùng self-signed cert) |
| JWT verify fail | `Jwt__Key` ≥ 32 ký tự, không khoảng trắng đầu/cuối |
| Email không gửi | `Smtp__Enabled=true` + App Password đúng + `Smtp__FromAddress` trùng Gmail user |

## Lệnh thường dùng

```powershell
# Push code mới → cả edutrack-api + edutrack-web auto-redeploy
git add -A
git commit -m "..."
git push

# Local test build prod (kiểm tra trước khi push):
cd EduTrack.WEB
npm run build:prod
# Static file ở dist/edutrack.web-prod/browser/
```
