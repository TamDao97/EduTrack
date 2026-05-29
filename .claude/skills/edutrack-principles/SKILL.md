---
name: edutrack-principles
description: Load skill này ngay khi bắt đầu BẤT KỲ task code nào trong repo EduTrack (D:\Coderkechuyen\EduTrack — Tutor SaaS cho gia sư VN). Đây là kim chỉ nam: code dễ hiểu + tổ chức thông minh + UI đẹp giữ chân người dùng. Áp dụng cho cả BE (.NET 8) lẫn FE (Angular 17). UI bắt buộc theo DESIGN_SYSTEM.md.
---

# EduTrack — Kim chỉ nam

> 3 mục tiêu xuyên suốt mọi file mình viết. Nếu phải chọn 1 trong 3, ưu tiên theo thứ tự: **dễ hiểu > tổ chức > đẹp**.

## 📐 Design System — quan trọng nhất khi làm UI

Mọi trang/component FE mới **BẮT BUỘC** tuân thủ `DESIGN_SYSTEM.md` (ở root repo).

**Bộ tokens + mixins**:
- `src/styles/_palette.scss` — color, typography, spacing, radius, shadow, breakpoint
- `src/styles/_mixins.scss` — hero-gradient, card-clickable, btn-primary, form-input, empty-state…
- Import: `@use 'palette' as *; @use 'mixins' as *;`

**Pattern chuẩn cho trang nghiệp vụ**:
```scss
.page-x {
  @include page-container;
  &__hero { @include hero-gradient; }
  &__content { @include content-sheet; }
}
.x-card { @include card-clickable; }
.x-card__highlight { color: $accent; font-weight: $fw-bold; }  // financial highlight
```

**KHÔNG được**: hardcode hex color, hardcode px spacing, copy-paste >50 dòng SCSS lặp lại pattern đã có mixin.

---

## 1. Code dễ hiểu, clean

**Naming:**
- Tên biến/hàm/class bằng tiếng Anh, PascalCase cho class/method, camelCase cho biến local + field private có `_` prefix (theo scaffold).
- Comment XML, error message, enum `[Description]`, UI label bằng tiếng Việt.
- Tên dài rõ nghĩa hơn tên ngắn mơ hồ: `idTutor` chứ không `tId`; `lessonsInPeriod` chứ không `lst`.
- Không viết tắt trừ khi cực phổ biến (`Id`, `Dto`, `Req`, `Res`, `Ctx`).

**Structure mỗi method:**
- ≤ 30 dòng. Dài hơn → tách helper private.
- Early return + guard clause, không nest if quá 3 cấp.
- 1 method = 1 lý do để thay đổi. Tách logic xác thực, biến đổi, lưu DB thành các method riêng nếu hợp lý.

**Comment đúng cách:**
- Comment giải thích **why**, code đã nói **what**. Đừng `// tăng x lên 1` cho `x++`.
- Comment business rule + edge case: vd `// snapshot ChargeAmount vì tutor có thể đổi PerLessonRate sau này`.
- Bỏ comment cũ không còn đúng — xoá tốt hơn để lạc lối.

**1 file = 1 thứ chính:**
- 1 entity / 1 service / 1 component / 1 page = 1 file. Tránh gom 5 service nhỏ vào 1 file `Misc.cs`.
- DTO liên quan có thể chung file (Dto + GridFilter + Req + DetailDto), nhưng KHÔNG mix với entity hay service.

---

## 2. Tổ chức code thông minh

**Folder theo domain ở mức trên cùng, theo type ở mức trong:**
```
Entity/TutorDomain/        Services/TutorDomain/        Controllers/TutorDomain/
  Parent.cs                  ParentService.cs              ParentController.cs
  Student.cs                 StudentService.cs             StudentController.cs
  ...                        ...                           ...
```
Khi cần tìm gì liên quan tới TutorDomain → mở 1 folder thấy hết. Đừng để Entity/Parent.cs nằm rời rạc cạnh File.cs (core).

**Base + extend, không copy-paste:**
- 4-5 service share logic → tách `TutorScopedBaseService<T,TDto>`. Tránh copy filter `IdTutor` vào từng service.
- 4-5 controller share routing/auth → đã có `ApiController` + `[TDAuthorize]`. Đừng tự viết permission check.

**Interface trước class:**
- Mọi service đều có `IXxxService` + `XxxService`. Inject qua DI.
- Test/mock dễ hơn, decoupling rõ hơn.

**DTO ≠ Entity:**
- Entity = DB shape. DTO = API shape. Map qua `AutoMapperGeneric.Map<TFrom, TTo>(obj)`.
- Đừng expose entity trực tiếp ra HTTP. Đừng nhận entity từ client.
- Khi cần extra field hiển thị (vd `ParentFullName` trên `StudentDetailDto`), tạo DTO mở rộng kế thừa DTO gốc.

**Single source of truth cho business rule:**
- Snapshot `ChargeAmount` từ `Student.PerLessonRate` → chỉ trong `LessonService.CreateAsync` và `BulkCreateRecurring`. Không lặp ở 2 nơi.
- Tính `OutstandingAmount = FinalAmount - PaidAmount` → 1 chỗ duy nhất (computed property trên entity).
- Quy tắc "lesson đã chốt kỳ thì không sửa" → check trong `LessonService.UpdateAsync` + `CancelAsync` + `MarkDoneAsync`, không để FE check.

**Bắt buộc nhớ ở mỗi entity nghiệp vụ:**
- Kế thừa `BaseEntity` (audit + soft delete + MarkDirty).
- Implement `ITutorScoped` + có cột `IdTutor` → filter tự động trong `TutorScopedBaseService`.
- Khi `UpdateAsync` phải `MarkDirty(nameof(field))` cho từng field thay đổi. Nếu không gọi MarkDirty → field đó KHÔNG được update vào DB (đây là quy ước scaffold).

**Frontend tổ chức:**
- `services/tutor-domain/` chứa Angular service 1 file / entity, extend `TdBaseService`.
- `pages/tutor-domain/{feature}/` chứa standalone component + sub-component liên quan.
- `interfaces/I{Entity}.ts` chứa TypeScript model match BE DTO.
- Shared logic dùng `shared/` (đừng tạo `helpers/` mới).

---

## 3. UI đẹp, giữ chân người dùng

> Tutor dùng ĐT khi đi dạy. App khó dùng = họ quay lại Excel + sổ tay. Mất user = mất revenue.

**Mobile-first bắt buộc:**
- Design ở viewport 375px (iPhone SE) trước, mở rộng lên desktop sau.
- Test bằng DevTools responsive mode khi build.
- KHÔNG hamburger menu — dùng bottom navigation (tay cầm ĐT 1 tay khó với menu trên).

**1-tap cho việc làm hàng ngày:**
- Mark "đã dạy" 1 buổi: 1 tap vào nút check trên card lesson.
- Gửi nhắc Zalo: 1 tap vào nút Zalo trên inbox card.
- Ghi nhận thanh toán: 1 tap vào nút "Đã nhận" trên row outstanding.
- KHÔNG bắt user mở modal → confirm → submit cho action hàng ngày. Confirm chỉ dành cho xoá/huỷ.

**Empty state phải dẫn hành động:**
```
[Chưa có học sinh nào]
[+  Thêm học sinh đầu tiên]   ← nút lớn primary
```
Không phải hình minh hoạ trống + dòng "No data".

**Status feedback nhất quán:**
- Xanh `#52c41a` = OK / Active / Paid / Done
- Vàng `#faad14` = Pending / PartialPaid / Sắp đến hạn
- Đỏ `#f5222d` = Cancelled / Overdue / Error
- Xám `#8c8c8c` = Stopped / Paused / Inactive

**Format dữ liệu VN:**
- Ngày: "thứ 3, 28/05/2026" hoặc "Hôm nay · 19h30-21h". KHÔNG `2026-05-28T19:30:00Z`.
- Tiền: `1.500.000đ`. KHÔNG `1500000` hay `1500000 VND`.
- Số điện thoại: `0901 234 567` (group 3-3-3). KHÔNG `0901234567`.
- Dùng pipe `currencyText`, `dateVi` đã có ở `shared/pipes/`.

**Loading + feedback bắt buộc:**
- Mọi async action có loading state. Skeleton loader cho grid (>10 item), spinner kèm text cho action.
- Toast cho mọi success/error. Đừng để tutor không biết action có thành công hay không.
- Optimistic update khi đơn giản (toggle status): cập nhật UI trước, rollback nếu API lỗi.

**Tap target ≥ 44×44px** (iOS HIG). Icon nhỏ → tăng padding xung quanh.

**Modal vs Bottom sheet:**
- Form ngắn (≤ 3 field): modal nhỏ center.
- Form dài / list option: bottom sheet drag-to-dismiss, full-screen mobile.

**Anti-pattern UI cần tránh:**
- ❌ Form siêu dài 1 trang. Chia step nếu > 6 field bắt buộc.
- ❌ Confirm popup cho mọi action ("Bạn có chắc muốn lưu?"). Chỉ confirm cho phá huỷ.
- ❌ Dùng `alert()` / `prompt()` native. Luôn dùng nz-modal / nz-notification.
- ❌ Màu sắc lộn xộn. Tối đa 5 màu chính trong app.
- ❌ Icon không có label trên màn hình quan trọng (mobile lần đầu vào dễ bối rối).

---

## Anti-pattern code chung

- ❌ Business logic trong controller. Controller chỉ dispatch sang service.
- ❌ Query thẳng DbContext trong controller hoặc component. Luôn qua service.
- ❌ `var` cho mọi thứ — dùng type tường minh khi readability tăng.
- ❌ Magic number / magic string. Dùng `enum` hoặc `const` trong `Constants.cs` / `constants.ts`.
- ❌ Catch + swallow exception. Log hoặc rethrow.
- ❌ N+1 query trong loop. Dùng `join` LINQ hoặc `Include`.
- ❌ FE subscribe trong template (`{{ obs$ | async }}` ok, nhưng `{{ method() }}` trigger re-call mỗi CD). Tính trong .ts, expose property.
- ❌ Inline style FE. CSS class trong file `.css`/`.scss`.

---

## Checklist trước khi commit

- [ ] Build BE pass: `dotnet build` 0 error
- [ ] Build FE pass: `npm run build` không error
- [ ] Mỗi method mới: tên rõ, ≤ 30 dòng, không deep nest
- [ ] Mỗi entity nghiệp vụ mới: kế thừa `BaseEntity` + implement `ITutorScoped` + có `IdTutor`
- [ ] Mỗi service nghiệp vụ mới: extend `TutorScopedBaseService` (trừ TutorProfile)
- [ ] Mỗi `UpdateAsync` mới: có `MarkDirty(nameof(field))` cho từng field
- [ ] Mỗi UI mới: test viewport 375px (iPhone SE), 1-tap action, loading + toast
- [ ] Không expose Entity ra HTTP — luôn map sang DTO
