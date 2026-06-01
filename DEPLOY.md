# EduTrack — Deploy production

> Mục tiêu: từ `git push develop` → app live trên Internet trong 30 phút. Free tier toàn bộ.

## Kiến trúc deploy

```
┌──────────────────────────────┐
│ Vercel (FE — Angular static) │
│ https://edutrack.vercel.app  │
└─────────────┬────────────────┘
              │ HTTPS calls
              ▼
┌──────────────────────────────┐
│ Render.com (BE — .NET 8 Docker)
│ https://edutrack-api.onrender.com
└─────────────┬────────────────┘
              │ SQL TCP 1433
              ▼
┌──────────────────────────────┐
│ Your VPS (SQL Server)        │
│ vps.ngxhuyhoang.com:1433     │
└──────────────────────────────┘
```

**Chi phí**: 0đ (Render free tier 750h/month, Vercel free unlimited static). VPS đã có sẵn.

**Trade-off Render free**: BE sleep sau 15 phút không request → request đầu sau ngủ mất ~10-20s wake up. OK cho beta, upgrade $7/mo khi cần always-on.

---

## Bước 1 — Push code mới nhất

```powershell
cd D:\Coderkechuyen\EduTrack
git push origin develop
```
Đảm bảo file `Dockerfile` (BE) + `vercel.json` + `render.yaml` đã có ở repo.

## Bước 2 — Deploy BE lên Render

1. Vào https://render.com → đăng ký free (sign in with GitHub)
2. **New +** → **Blueprint** → connect repo `TamDao97/EduTrack`
3. Render đọc `render.yaml` → tạo service `edutrack-api`
4. Vào service vừa tạo → tab **Environment** → **Add Environment Variable**:

| Key | Value | Secret? |
|---|---|---|
| `ConnectionStrings__EduTrackDbContextConnection` | `Server=vps.ngxhuyhoang.com,1433;Database=EduTrack;User Id=sa;Password=YOUR_PASS;Encrypt=false;TrustServerCertificate=true` | ✅ |
| `Jwt__Key` | chuỗi random ≥ 32 ký tự, vd `edutrack-prod-aB1c2D3e4F5g6H7i8J9k0L-2026-secret-key` | ✅ |
| `Cors__AllowedOrigins` | tạm `http://localhost:4200` — sẽ update sau bước 4 | – |

5. **Manual Deploy** → đợi build (~5-8 phút lần đầu)
6. Khi log hiện `Now listening on http://+:8080`, service sẵn sàng. Copy URL Render assign (vd `https://edutrack-api-xxxx.onrender.com`)
7. Test: `https://edutrack-api-xxxx.onrender.com/health` → trả `{"status":"ok",...}`

## Bước 3 — Apply DB migration (lần đầu)

VPS SQL Server đã có DB `EduTrack` (đã setup local). Chạy lại schema cho chắc:

```powershell
# Trên máy local
cd D:\Coderkechuyen\EduTrack\EduTrack.API
$env:ConnectionStrings__EduTrackDbContextConnection = "<production connection string>"
dotnet ef database update
```

Hoặc dùng SSMS chạy `ScriptSql/Schema/1_InitDb.sql` + `ScriptSql/Seed/2_Seed_InitData.sql`.

## Bước 4 — Deploy FE lên Vercel

1. Vào https://vercel.com → đăng ký free (sign in with GitHub)
2. **Add New** → **Project** → import repo `TamDao97/EduTrack`
3. Configure:
   - **Root Directory**: `EduTrack.WEB`
   - **Framework Preset**: Angular (auto-detect)
   - **Build Command**: `npm run build:prod` (đã set trong `vercel.json`)
   - **Output Directory**: `dist/edutrack.web-prod/browser`
4. **Deploy** → đợi ~3 phút
5. Vercel assign URL vd `https://edutrack-frontend-tamdao97.vercel.app`

## Bước 5 — Cập nhật API URL trong FE

Sửa `EduTrack.WEB/src/environment.production.ts`:

```typescript
export const environment = {
    production: true,
    apiUrl: 'https://edutrack-api-xxxx.onrender.com/api',  // ← URL Render
    fileUrl: 'https://edutrack-api-xxxx.onrender.com',
};
```

Commit + push → Vercel auto-redeploy.

## Bước 6 — Cập nhật CORS cho BE

Quay lại Render → service `edutrack-api` → Environment → sửa `Cors__AllowedOrigins`:

```
http://localhost:4200,https://edutrack-frontend-tamdao97.vercel.app
```

Save → service restart.

## Bước 7 — Smoke test prod

1. Mở `https://edutrack-frontend-tamdao97.vercel.app`
2. Click **Đăng ký miễn phí** → tạo account `tutor1@beta.com / 123456`
3. Nếu thấy "Đăng ký thành công" + redirect `/dashboard` → ✅
4. Test thêm 1 HS, 1 buổi, mark done, chốt kỳ
5. Inbox check Zalo deeplink mở đúng

Nếu lỗi:
- **F12 Console** xem CORS lỗi gì
- **Render Logs** xem BE error
- **Network tab** xem request có tới đúng API URL không

## Bước 8 — Custom domain (tuỳ chọn)

Khi sẵn sàng "ra mắt" với domain `edutrack.app` (đăng ký Namecheap/Cloudflare ~150k/năm):

1. Vercel project → Domains → add `edutrack.app` → set DNS record theo hướng dẫn
2. Render service → Settings → Custom Domains → add `api.edutrack.app` → set CNAME
3. Sửa `environment.production.ts` apiUrl thành `https://api.edutrack.app/api`
4. Update Render `Cors__AllowedOrigins` thêm `https://edutrack.app`

## Bước 9 — Onboard 5-10 tutor beta

- Tạo 1 tin nhắn Zalo / Facebook gửi cho gia sư quen biết:
  > "Em đang làm app quản lý gia sư. Bạn dùng giúp em 1 tuần feedback nhé? Link: https://edutrack-frontend-tamdao97.vercel.app — đăng ký free, 14 ngày không thẻ. Có gì khó dùng nhắn em sửa ngay."
- Track funnel: bao nhiêu signup, bao nhiêu thêm HS đầu, bao nhiêu tạo lesson, bao nhiêu chốt kỳ
- Sau 1 tuần survey 3 câu: thích nhất / ghét nhất / sẵn sàng trả 99k/tháng nếu có 5 tính năng X Y Z?

---

## Troubleshooting

| Triệu chứng | Fix |
|---|---|
| BE Render build fail "Cannot find Base.Lib" | Dockerfile context phải là root repo (đã set `dockerContext: .` trong render.yaml) |
| FE Vercel build fail "ng: command not found" | Check vercel.json has `installCommand: npm install --legacy-peer-deps` |
| CORS lỗi từ FE | Check Render env var `Cors__AllowedOrigins` chính xác URL Vercel (no trailing slash) |
| 504 timeout request đầu | BE Render sleep — chấp nhận hoặc upgrade $7/mo |
| SQL connection timeout từ Render → VPS | VPS firewall phải allow Render IPs. Hoặc dùng Render PostgreSQL free instead — cần migrate DB |
| JWT verify fail | `Jwt__Key` ≥ 32 ký tự, không có khoảng trắng đầu/cuối |

## Lệnh thường dùng sau khi deploy

```powershell
# Push code mới → cả Render + Vercel auto-redeploy
git add -A
git commit -m "..."
git push

# Xem Render logs (qua dashboard) hoặc Render CLI:
# https://render.com/docs/cli

# Local test build prod:
cd EduTrack.WEB
npm run build:prod
# Static file ở dist/edutrack.web-prod/browser/
```
