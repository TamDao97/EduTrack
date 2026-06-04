import { Injectable, signal } from '@angular/core';
import { IToast, ToastType } from '../components/toast/toast.model';

/**
 * Toast tự xây (thay NzNotification) — render qua ToastHostComponent đặt ở AppComponent.
 * API public giữ nguyên (success/error/warning/info(title, content)) nên 100% chỗ gọi cũ
 * không phải sửa. Tính năng: stack tối đa 5, hover tạm dừng đếm giờ, chống toast trùng,
 * progress bar đồng bộ với timer.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private static readonly MAX_STACK = 5;
  private static readonly LEAVE_MS = 220; // khớp thời lượng animation rời đi trong SCSS

  private _seq = 0;
  /** Timer + mốc hết hạn của từng toast — để hover pause/resume chính xác. */
  private _timers = new Map<number, { handle: any; endsAt: number; remaining: number }>();

  /** Danh sách toast đang hiển thị (mới nhất trên cùng). Host component render từ đây. */
  readonly toasts = signal<IToast[]>([]);

  success(title: string, content: string) { this.show('success', title, content, 3000); }
  error(title: string, content: string)   { this.show('error',   title, content, 5000); }
  warning(title: string, content: string) { this.show('warning', title, content, 4000); }
  info(title: string, content: string)    { this.show('info',    title, content, 3500); }

  show(type: ToastType, title: string, message: string, duration = 3500): void {
    // Chống spam trùng: cùng type+title+message đang hiện → chỉ refresh timer
    const dup = this.toasts().find(t => !t.leaving && t.type === type && t.title === title && t.message === message);
    if (dup) {
      this.clearTimer(dup.id);
      this.schedule(dup.id, dup.duration);
      return;
    }

    const toast: IToast = { id: ++this._seq, type, title, message, duration, leaving: false };
    this.toasts.update(list => {
      const next = [toast, ...list];
      // Quá stack → đẩy toast cũ nhất (chưa leaving) rời đi
      const overflow = next.filter(t => !t.leaving).slice(ToastService.MAX_STACK);
      overflow.forEach(t => this.dismiss(t.id));
      return next;
    });
    this.schedule(toast.id, duration);
  }

  /** Đóng 1 toast: chạy animation rời đi rồi mới remove khỏi list. */
  dismiss(id: number): void {
    this.clearTimer(id);
    const exists = this.toasts().some(t => t.id === id && !t.leaving);
    if (!exists) return;
    this.toasts.update(list => list.map(t => (t.id === id ? { ...t, leaving: true } : t)));
    setTimeout(() => {
      this.toasts.update(list => list.filter(t => t.id !== id));
    }, ToastService.LEAVE_MS);
  }

  /** Đóng tất cả. */
  clear(): void {
    this.toasts().forEach(t => this.dismiss(t.id));
  }

  /** Hover vào toast → dừng đếm giờ (progress bar pause bằng CSS :hover). */
  pause(id: number): void {
    const t = this._timers.get(id);
    if (!t) return;
    clearTimeout(t.handle);
    t.remaining = Math.max(0, t.endsAt - Date.now());
  }

  /** Rời chuột → đếm tiếp phần thời gian còn lại. */
  resume(id: number): void {
    const t = this._timers.get(id);
    if (!t) return;
    this.schedule(id, t.remaining);
  }

  private schedule(id: number, ms: number): void {
    this.clearTimer(id);
    this._timers.set(id, {
      handle: setTimeout(() => this.dismiss(id), ms),
      endsAt: Date.now() + ms,
      remaining: ms,
    });
  }

  private clearTimer(id: number): void {
    const t = this._timers.get(id);
    if (t) clearTimeout(t.handle);
    this._timers.delete(id);
  }
}
