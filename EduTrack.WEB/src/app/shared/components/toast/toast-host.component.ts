import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { ToastService } from '../../services/toast.service';
import { IToast, ToastType } from './toast.model';

/**
 * Host render toàn bộ toast — đặt 1 lần duy nhất trong AppComponent.
 * Mọi nơi khác chỉ gọi ToastService.success/error/warning/info như cũ.
 */
@Component({
  selector: 'app-toast-host',
  templateUrl: './toast-host.component.html',
  styleUrls: ['./toast-host.component.scss'],
  standalone: true,
  imports: [CommonModule, NzIconModule],
})
export class ToastHostComponent {
  private _toast = inject(ToastService);

  toasts = this._toast.toasts;

  icon(type: ToastType): string {
    return ({
      success: 'check-circle',
      error:   'close-circle',
      warning: 'warning',
      info:    'info-circle',
    } as Record<ToastType, string>)[type];
  }

  onDismiss(t: IToast) { this._toast.dismiss(t.id); }
  onEnter(t: IToast)   { this._toast.pause(t.id); }
  onLeave(t: IToast)   { this._toast.resume(t.id); }

  trackById(_: number, t: IToast) { return t.id; }
}
