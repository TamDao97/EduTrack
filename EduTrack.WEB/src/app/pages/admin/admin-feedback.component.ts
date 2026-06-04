import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { FeedbackStatus, FeedbackStatusLabel, FeedbackType, FeedbackTypeLabel, IFeedbackDetail } from '../../interfaces/IFeedback';
import { FeedbackService } from '../../services/tutor-domain/feedback.service';
import { SharedModule } from '../../shared/modules/shared.module';
import { ToastService } from '../../shared/services/toast.service';
import { StatusResponseTitle } from '../../shared/utils/constants';
import { StatusCode } from '../../shared/utils/enums';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

/**
 * Admin Góp ý — founder xem toàn bộ góp ý của tutor, đổi trạng thái + phản hồi.
 * Route /admin/feedback. Dùng lại style admin-dashboard cho đồng bộ console.
 */
@Component({
  selector: 'app-admin-feedback',
  templateUrl: './admin-feedback.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class AdminFeedbackComponent extends TdBaseComponent implements OnInit {
  FeedbackType = FeedbackType;
  FeedbackTypeLabel = FeedbackTypeLabel;
  FeedbackStatus = FeedbackStatus;
  FeedbackStatusLabel = FeedbackStatusLabel;

  private _service = inject(FeedbackService);
  private _toast = inject(ToastService);

  items: IFeedbackDetail[] = [];
  totalRecord = 0;
  isLoading = false;
  savingId: string | null = null;
  /** id dòng đang mở phần phản hồi */
  expandedId: string | null = null;
  /** buffer chỉnh sửa: id → { status, adminNote } */
  edit: { [id: string]: { status: FeedbackStatus; adminNote: string } } = {};

  filter: { pageNumber: number; pageSize: number; type: FeedbackType | null; status: FeedbackStatus | null } =
    { pageNumber: 1, pageSize: 50, type: null, status: null };

  statusTabs: { value: FeedbackStatus | null; label: string }[] = [
    { value: null, label: 'Tất cả' },
    { value: FeedbackStatus.Moi,        label: FeedbackStatusLabel[FeedbackStatus.Moi] },
    { value: FeedbackStatus.DangXemXet, label: FeedbackStatusLabel[FeedbackStatus.DangXemXet] },
    { value: FeedbackStatus.SeLam,      label: FeedbackStatusLabel[FeedbackStatus.SeLam] },
    { value: FeedbackStatus.DaLam,      label: FeedbackStatusLabel[FeedbackStatus.DaLam] },
    { value: FeedbackStatus.TuChoi,     label: FeedbackStatusLabel[FeedbackStatus.TuChoi] },
  ];

  typeOptions = [
    { value: null, label: 'Mọi loại' },
    { value: FeedbackType.GopY,     label: FeedbackTypeLabel[FeedbackType.GopY] },
    { value: FeedbackType.BaoLoi,   label: FeedbackTypeLabel[FeedbackType.BaoLoi] },
    { value: FeedbackType.TinhNang, label: FeedbackTypeLabel[FeedbackType.TinhNang] },
    { value: FeedbackType.Khac,     label: FeedbackTypeLabel[FeedbackType.Khac] },
  ];

  statusOptions = [
    FeedbackStatus.Moi, FeedbackStatus.DangXemXet, FeedbackStatus.SeLam,
    FeedbackStatus.DaLam, FeedbackStatus.TuChoi,
  ];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    this._service.getByFilter(this.filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this.items = rs.data?.data ?? [];
            this.totalRecord = rs.data?.totalRecord ?? 0;
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được danh sách góp ý'),
      });
  }

  onSelectStatus(v: FeedbackStatus | null): void {
    this.filter.status = v;
    this.filter.pageNumber = 1;
    this.load();
  }

  onTypeChange(v: FeedbackType | null): void {
    this.filter.type = v;
    this.filter.pageNumber = 1;
    this.load();
  }

  toggleReply(f: IFeedbackDetail): void {
    if (this.expandedId === f.id) { this.expandedId = null; return; }
    this.expandedId = f.id!;
    if (!this.edit[f.id!]) {
      this.edit[f.id!] = { status: f.status ?? FeedbackStatus.Moi, adminNote: f.adminNote ?? '' };
    }
  }

  onSave(f: IFeedbackDetail): void {
    const e = this.edit[f.id!];
    if (!e) return;
    this.savingId = f.id!;
    this._service.updateStatus(f.id!, e.status, e.adminNote?.trim() || null)
      .pipe(finalize(() => this.savingId = null))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, 'Đã cập nhật góp ý');
            this.expandedId = null;
            this.load();
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  statusClass(s?: FeedbackStatus): string {
    return ({
      [FeedbackStatus.Moi]:        'tag-pro',
      [FeedbackStatus.DangXemXet]: 'tag-basic',
      [FeedbackStatus.SeLam]:      'tag-basic',
      [FeedbackStatus.DaLam]:      'tag-free',
      [FeedbackStatus.TuChoi]:     'tag-free',
    } as any)[s ?? FeedbackStatus.Moi];
  }

  fmtDate(s?: string): string {
    if (!s) return '—';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }

  trackById(_: number, f: IFeedbackDetail) { return f.id; }
}
