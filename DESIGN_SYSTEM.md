# EduTrack — Design System

> Tài liệu chuẩn UI cho toàn bộ phần mềm. Bất kỳ trang mới nào cũng tuân theo tài liệu này để giữ tính đồng nhất, đẹp mắt, sạch.

## Triết lý (3 nguyên tắc)

1. **Mobile-first, 1-tap actions** — Tutor dùng điện thoại khi đi dạy. Mọi action hàng ngày (mark done, gửi nhắc, thu tiền) phải ≤ 2 tap.
2. **Hierarchy bằng độ tương phản, không phải đường viền** — Card nổi bật bằng shadow + spacing, không phải border đậm. Giữ background sạch (off-white #F7F8FC).
3. **Mỗi pixel có lý do** — không trang trí thừa. Mọi animation, gradient, glow đều phục vụ chức năng (nhận diện status, focal point, depth).

---

## Tokens (đã có sẵn trong `src/styles/_palette.scss`)

Mọi file SCSS dùng `@use 'palette' as *;`. **Không bao giờ** viết hex code thẳng — luôn qua biến token.

### Color

| Token | Hex | Dùng cho |
|---|---|---|
| `$primary` | `#5B5FCF` | Indigo — nút chính, link, focus ring, hero gradient start |
| `$primary-soft` | `#E8EAFF` | Background hover, chip môn học |
| `$primary-vivid` | `#7C3AED` | Violet — hero gradient end |
| `$accent` | `#FF8A65` | Coral — **financial highlight** (giá tiền, doanh thu, badge khẩn) |
| `$accent-soft` | `#FFEEE8` | Background chip lớp/grade |
| `$success` | `#52C41A` | Status Active, Done, Paid |
| `$warning` | `#FAAD14` | Status Paused, Pending, sắp đến hạn |
| `$danger` | `#F5222D` | Cancelled, Overdue, hành động xoá |
| `$zalo` | `#0068FF` | Nút/icon liên quan Zalo |
| `$bg` | `#F7F8FC` | Background page |
| `$surface` | `#FFFFFF` | Card, modal, panel |
| `$border` | `#E8EAF3` | Viền mặc định |
| `$text-1/2/3/4` | dark → light | 4 cấp text hierarchy |

### Typography scale

| Token | Size | Dùng cho |
|---|---|---|
| `$fs-display` | 28px (mobile) / 32px (desktop) | Hero title (chỉ trang chính) |
| `$fs-h1` | 22px | Page title trong header |
| `$fs-h2` | 18px | Section title |
| `$fs-h3` | 16px | Card name, list item title |
| `$fs-body-lg` | 15px | Form input, paragraph |
| `$fs-body` | 14px | Body chuẩn |
| `$fs-sm` | 13px | Helper, label |
| `$fs-xs` | 12px | Meta, parent info |
| `$fs-caption` | 11px | Eyebrow uppercase, count badge |

Weight: `$fw-normal` 400 · `$fw-medium` 500 · `$fw-semi` 600 · `$fw-bold` 700

### Spacing (scale 4px)

| Token | Px |
|---|---|
| `$sp-1` | 4 |
| `$sp-2` | 8 |
| `$sp-3` | 12 |
| `$sp-4` | 16 |
| `$sp-5` | 20 |
| `$sp-6` | 24 |
| `$sp-8` | 32 |
| `$sp-10` | 40 |
| `$sp-12` | 48 |

**Quy tắc**: padding/margin luôn dùng token, không tự viết `padding: 17px`. Nếu cần giá trị ngoài scale (rất hiếm), comment lý do.

### Radius

- `$radius-sm` 8px — chip, status pill
- `$radius` 10px — input, button
- `$radius-md` 12px — card chính
- `$radius-lg` 16px — modal mobile, content sheet
- `$radius-2xl` 24px — hero card desktop
- `$radius-pill` ∞ — pill button

### Shadow (5 cấp)

```
$shadow-xs   →  subtle (skeleton, divider)
$shadow      →  card resting
$shadow-md   →  hover medium
$shadow-lg   →  hover prominent (card hover)
$shadow-xl   →  modal, FAB

$shadow-primary  →  cho element màu primary (CTA, FAB)
$shadow-accent   →  cho element màu accent (badge khẩn)
$shadow-zalo     →  cho nút Zalo
```

### Gradient

```
$gradient-hero      →  indigo → violet (header trang)
$gradient-primary   →  indigo → dark indigo (CTA, FAB)
$gradient-accent    →  coral nhạt → coral (financial CTA)
```

---

## Mixins (đã có sẵn trong `src/styles/_mixins.scss`)

Dùng `@use 'mixins' as *;` để gọi mixin trực tiếp.

### Typography

```scss
.title { @include t-h1; }
.eyebrow { @include t-eyebrow; }
```

### Layout

```scss
.page-students      { @include page-container; }
.page-students__hero { @include hero-gradient; }
.page-students__content { @include content-sheet; }
```

### Card

```scss
.product-card { @include card-clickable; }   // hover effect
.empty-box    { @include card-ghost; }       // dashed border
```

### Button

```scss
.btn-save     { @include btn-primary; }       // gradient, 44px
.btn-cancel   { @include btn-secondary; }
.btn-delete   { @include btn-danger; }
.btn-icon-x   { @include btn-icon(38px); }
.btn-add-fab  { @include btn-fab; }           // fixed, chỉ mobile
```

### Form

```scss
.field-input { @include form-input; }
.field-label { @include form-label; }
```

### Empty state

```scss
.empty {
  @include empty-state;
  &__icon { @include empty-icon-circles(160px, $primary); }
}
```

### Skeleton

```scss
.s-line  { @include skeleton(12px, 70%); }
.s-circle { @include skeleton(52px, 52px, 14px); }
```

### Utility

```scss
.name        { @include truncate; }              // 1 dòng
.description { @include truncate-lines(2); }     // n dòng
.glass-pill  { @include glass; }                 // glass-morphism
```

---

## Layout pattern chuẩn (mỗi trang nghiệp vụ)

```
┌─────────────────────────────────────┐
│  HERO (indigo→violet gradient)      │   ← @include hero-gradient
│  - Eyebrow uppercase (optional)     │
│  - Page title h1                    │
│  - Action button (glass-morphism)   │
│  - Stats / nav strip                │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│  CONTENT SHEET (đè lên hero −16px)  │   ← @include content-sheet
│  - Search + Filter tabs             │
│  - List cards / Grid                │
│  - Sections                         │
└─────────────────────────────────────┘
                                  [+ FAB] ← @include btn-fab (mobile only)
```

Pattern này áp dụng cho: `/student`, `/lesson`, `/inbox`, sau này `/tuition`, `/report`.

### Trang detail

```
┌─────────────────────────────────────┐
│  HEADER (gradient, có nút Back)     │
│  + HERO portion (avatar + name)     │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│  STATS strip (chìm xuống −20px)     │   ← stat-card grid
│  SECTIONS (Parent, Notes, Schedule…)│
└─────────────────────────────────────┘
```

---

## Component patterns

### 1. Card (item trong list)

```html
<li class="entity-card">
  <div class="entity-card__avatar"> … </div>
  <div class="entity-card__body">
    <div class="entity-card__head">
      <span class="entity-card__name">{{ name }}</span>
    </div>
    <div class="entity-card__chips"> <span class="chip chip--primary">…</span> </div>
    <div class="entity-card__highlight">{{ moneyHighlight }}</div>
    <div class="entity-card__meta">{{ meta }}</div>
  </div>
  <div class="entity-card__actions">
    <button class="action-btn"> … </button>
  </div>
</li>
```

Quy ước class:
- `__avatar` — square 52×52 với gradient hash-color + status dot góc dưới-phải
- `__body` — flex 1, min-width 0 (để `__name` truncate đúng)
- `__head` — chứa tên + optional badge
- `__chips` — 1-3 chip tag (subject, grade, …)
- `__highlight` — số tiền/giá trị nổi (font coral 15px bold)
- `__meta` — phụ huynh / địa chỉ / SĐT (12px gray)
- `__actions` — 2-3 nút icon 38×38

### 2. Button hierarchy

| Loại | Khi nào dùng | Visual |
|---|---|---|
| Primary | Hành động chính của trang (Save, Add, Submit) | Gradient indigo + shadow |
| Secondary | Action phụ (Cancel, Back) | White + border |
| Ghost | Action tertiary (Clear filter, View more) | Transparent + primary color |
| Danger | Xoá / Huỷ vĩnh viễn | Solid red |
| Icon | Inline trong card/header | 38×38 square |
| FAB | Add ở mobile | Tròn 56×56 cố định góc |

**1 hierarchy / 1 màn**: tối đa 1 nút Primary nổi bật ở mỗi view. Các nút khác phải secondary/ghost.

### 3. Form layout

```html
<form nz-form [formGroup]="frm" nzLayout="vertical">
  <nz-form-item>
    <nz-form-label nzRequired>Họ và tên</nz-form-label>
    <nz-form-control nzErrorTip="Vui lòng nhập">
      <input nz-input formControlName="name" placeholder="Nguyễn Minh Mai" />
    </nz-form-control>
  </nz-form-item>

  <div nz-row [nzGutter]="12">
    <div nz-col [nzSpan]="12">…</div>
    <div nz-col [nzSpan]="12">…</div>
  </div>

  <div class="form-actions">     <!-- sticky bottom -->
    <button nz-button nzSize="large">Huỷ</button>
    <button nz-button nzType="primary" nzSize="large">Lưu</button>
  </div>
</form>
```

Rules:
- Label luôn trên input (`nzLayout="vertical"`) — mobile-friendly
- Required field có asterisk đỏ tự động
- 2 field cùng nhóm gom vào 1 row `nz-row + nz-col` (vd: từ ngày / số tuần)
- Action buttons sticky bottom với border-top + flex justify-end
- Mobile (≤767px): 2 nút action stretch full với `flex: 1`

### 4. Status indicators

```scss
.tag--success  { background: $success-soft; color: $success-dark; }
.tag--warning  { background: $warning-soft; color: $warning-dark; }
.tag--danger   { background: $danger-soft; color: $danger-dark; }
.tag--neutral  { background: $bg-strong; color: $text-2; }
```

Dot indicator (Slack-style trên avatar):
```scss
.dot { @include status-dot($success, 14px); border: 2.5px solid $surface; }
```

### 5. Empty state

```html
<div class="empty">
  <div class="empty__art"> <span nz-icon nzType="team"></span> </div>
  <h3 class="empty__title">Bắt đầu hành trình dạy học</h3>
  <p class="empty__desc">Thêm học sinh đầu tiên để…</p>
  <button class="empty__cta"> <span nz-icon nzType="plus"></span> Thêm HS đầu tiên </button>
</div>
```

Rules:
- **Luôn có CTA** nếu có thể tạo entity
- Title nói tích cực ("Bắt đầu…") không tiêu cực ("No data")
- Icon trong 3 vòng tròn concentric tăng dần opacity — tạo focal point
- CTA dùng gradient + shadow primary

### 6. Modal vs Bottom sheet

```typescript
// Form (>2 field): bottom-sheet trên mobile, modal center trên desktop
this.openModal(
  { title: 'Thêm HS', width: 560, className: 'sheet-bottom-mobile' },
  StudentFormComponent,
  {}
);
```

Class `sheet-bottom-mobile` đã có sẵn (mixin `sheet-bottom-mobile`). Tự động:
- Mobile: trượt từ dưới lên, full-width, border-radius trên 20px
- Desktop: modal center, width 560px

### 7. Toast feedback

```typescript
this._toast.success(StatusResponseTitle.SUCCESS, 'Đã thêm học sinh');
this._toast.warning(StatusResponseTitle.WARNING, 'Vui lòng nhập thông tin');
this._toast.error(StatusResponseTitle.ERROR, rs.message);
```

**Mọi async action thành công/lỗi** → toast. Đừng để user không biết kết quả.

---

## Animation rules

Mọi transition dùng token:
- `$t-fast` 120ms — hover icon, focus ring
- `$t` 180ms — card hover, color change
- `$t-slow` 300ms — modal open/close

Easing:
- `$ease-out` — hầu hết (transition vào)
- `$ease-spring` — cho element xuất hiện (toast, fab grow)

Card hover lift cố định:
```scss
&:hover {
  transform: translateY(-2px);
  box-shadow: $shadow-lg;
}
&:active { transform: scale(0.99); }
```

---

## Format dữ liệu VN (bắt buộc, không exception)

| Loại | Format | KHÔNG |
|---|---|---|
| Tiền | `1.500.000đ` hoặc `1.500.000 đ` | `1500000 VND`, `1500000` |
| Ngày | `thứ 3, 28/05/2026` hoặc `28/05/2026` | `2026-05-28`, `5/28/26` |
| Giờ | `19h30` hoặc `19:30` | `7:30 PM` |
| SĐT | `0901 234 567` (group 3-3-3) | `0901234567` |
| Khoảng thời gian tương đối | `5p nữa`, `2h trước`, `Hôm nay`, `Mai` | "in 5 minutes" |

Helper có sẵn:
- Pipe `number:'1.0-0'` cho tiền VN
- `formatPhone()` trong các component
- `fmtScheduledAt()` trong notification-inbox cho relative time

---

## Mobile-first checklist (mọi trang mới)

- [ ] Viewport test 375px (iPhone SE) trước desktop
- [ ] Mọi tap target ≥ 38×38px (icon button) hoặc ≥ 44px height (text button)
- [ ] Modal có class `sheet-bottom-mobile`
- [ ] Sticky header dùng `position: sticky; top: 0; z-index: $z-sticky`
- [ ] Horizontal scroll cho tabs/chips dùng `overflow-x: auto` + ẩn scrollbar
- [ ] Hero có gradient (không nền trắng phẳng)
- [ ] Empty state có CTA + hình minh hoạ (không text trơ)
- [ ] FAB chỉ hiện mobile (`@include tablet-up { display: none; }`)
- [ ] List items có hover (desktop) + active scale (mobile feedback)

---

## Anti-pattern — KHÔNG bao giờ làm

❌ **Không** hardcode hex code trong component SCSS. Dùng token.

```scss
// ❌ SAI
.card { background: #FFFFFF; border: 1px solid #E8EAF3; }

// ✅ ĐÚNG
.card { background: $surface; border: 1px solid $border; }
```

❌ **Không** dùng `px` cho spacing trừ khi đặc biệt. Dùng `$sp-*`.

❌ **Không** copy-paste 100+ dòng SCSS. Nếu thấy lặp lại pattern → tạo mixin.

❌ **Không** dùng `alert()` / `confirm()` native. Luôn `nz-modal` hoặc `this.confirmModal()`.

❌ **Không** dùng inline style. CSS class trong file `.scss`.

❌ **Không** mix tiếng Anh-Việt trong UI label. Tiếng Việt hết.

❌ **Không** dùng hamburger menu (☰). Bottom navigation hoặc rail sidebar.

❌ **Không** confirm popup cho action thường xuyên (toggle status). Chỉ confirm cho **phá huỷ** (xoá, huỷ buổi đã chốt).

❌ **Không** form 1 trang dài hơn 6 field required. Chia step hoặc dùng accordion.

❌ **Không** > 2 màu primary trong 1 view. Indigo + Coral là MAX, các màu khác làm accent nhỏ.

---

## Quick reference — Khi tạo trang mới

```scss
// my-feature.component.scss
@use 'palette' as *;
@use 'mixins' as *;

.page-feature {
  @include page-container;

  &__hero {
    @include hero-gradient;
  }

  &__content {
    @include content-sheet;
    padding: $sp-4 $sp-3;
  }

  &__list {
    list-style: none;
    margin: 0; padding: 0;
    display: flex; flex-direction: column;
    gap: $sp-2;
  }
}

.feature-card {
  @include card-clickable;
  display: flex;
  gap: $sp-3;
}

.feature-card__name { @include t-h3; }
.feature-card__meta { @include t-meta; }
.feature-card__highlight {
  color: $accent;
  font-size: $fs-body-lg;
  font-weight: $fw-bold;
}
```

```html
<div class="page-feature">
  <header class="page-feature__hero">
    <span class="t-eyebrow">QUẢN LÝ</span>
    <h1>Tên feature</h1>
  </header>
  <main class="page-feature__content">
    <ul class="page-feature__list">
      <li class="feature-card" *ngFor="…">…</li>
    </ul>
  </main>
  <button class="btn-fab" (click)="onAdd()">…</button>
</div>
```

---

## File reference

| File | Role |
|---|---|
| `src/styles/_palette.scss` | **Source of truth** — tokens (color, typography, spacing, shadow, radius, breakpoint, z-index, transition) |
| `src/styles/_mixins.scss` | Reusable mixins (hero-gradient, card, btn-*, form-input, empty-state, …) |
| `DESIGN_SYSTEM.md` (file này) | Spec human-readable |
| `.claude/skills/edutrack-principles/SKILL.md` | Skill cho Claude — link tới design system |

Khi thay đổi token, sửa `_palette.scss` 1 chỗ → toàn app tự cập nhật.
