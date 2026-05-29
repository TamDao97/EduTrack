import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { INotification, NotificationStatus, NotificationStatusLabel, NotificationType, NotificationTypeLabel } from '../../../interfaces/INotification';
import { NotificationService } from '../../../services/tutor-domain/notification.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-notification-inbox',
  templateUrl: './notification-inbox.component.html',
  styleUrls: ['./notification-inbox.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
  providers: [DatePipe],
})
export class NotificationInboxComponent extends TdBaseComponent implements OnInit {
  NotificationStatus = NotificationStatus;
  NotificationStatusLabel = NotificationStatusLabel;
  NotificationType = NotificationType;
  NotificationTypeLabel = NotificationTypeLabel;

  private _service = inject(NotificationService);
  private _toast = inject(ToastService);

  items: INotification[] = [];
  pending: INotification[] = [];
  sent: INotification[] = [];
  isLoading = false;
  expandedIds = new Set<string>();

  ngOnInit() { this.load(); }

  load() {
    this.isLoading = true;
    this._service.getInbox()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this.items = rs.data ?? [];
            this.pending = this.items.filter(n => n.status === NotificationStatus.Pending);
            this.sent = this.items.filter(n => n.status === NotificationStatus.Sent);
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được hộp nhắc'),
      });
  }

  /** Mở Zalo deeplink, sau khi mở thì popup hỏi "đã gửi chưa". */
  onSendZalo(n: INotification) {
    if (n.zaloDeepLink) window.open(n.zaloDeepLink, '_blank');
    // Mark sent ngay — tutor đã chủ động click Gửi
    this._service.markSent(n.id!).subscribe({
      next: (rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, 'Đã đánh dấu đã gửi');
          this.load();
        }
      },
    });
  }

  onCopyText(n: INotification) {
    navigator.clipboard.writeText(n.bodyText).then(() => {
      this._toast.success(StatusResponseTitle.SUCCESS, 'Đã copy nội dung');
    });
  }

  onMarkSent(n: INotification) {
    this._service.markSent(n.id!).subscribe({
      next: (rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, 'Đã đánh dấu đã gửi');
          this.load();
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      },
    });
  }

  toggleExpand(id: string) {
    if (this.expandedIds.has(id)) this.expandedIds.delete(id);
    else this.expandedIds.add(id);
  }

  isExpanded(id?: string): boolean { return id ? this.expandedIds.has(id) : false; }

  /** "T2 27/05 · 19h00" — relative-ish */
  fmtScheduledAt(d: string): string {
    if (!d) return '';
    const date = new Date(d);
    const now = new Date();
    const diffMin = Math.round((date.getTime() - now.getTime()) / 60000);
    if (diffMin > -60 && diffMin <= 60) return diffMin <= 0 ? `${Math.abs(diffMin)}p trước` : `${diffMin}p nữa`;
    if (diffMin > 60 && diffMin < 24 * 60) return `${Math.round(diffMin / 60)}h nữa`;
    if (diffMin < -60 && diffMin > -24 * 60) return `${Math.round(Math.abs(diffMin) / 60)}h trước`;
    const d2 = `${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}`;
    const t = `${String(date.getHours()).padStart(2, '0')}h${String(date.getMinutes()).padStart(2, '0')}`;
    return `${d2} · ${t}`;
  }

  typeIcon(t: NotificationType): string {
    if (t === NotificationType.LessonReminderEvening || t === NotificationType.LessonReminderHourBefore) return 'schedule';
    if (t === NotificationType.TuitionIssued) return 'dollar';
    return 'bell';
  }

  trackById(_: number, n: INotification) { return n.id; }
}
