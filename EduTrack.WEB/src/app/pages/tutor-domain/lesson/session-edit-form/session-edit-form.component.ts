import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { finalize } from 'rxjs';
import { LessonService } from '../../../../services/tutor-domain/lesson.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/**
 * Modal "Sửa cả ca" — đổi ngày/giờ/địa điểm đồng loạt mọi buổi Scheduled cùng GroupKey.
 * params: { groupKey, className, scheduledDate, startTime, endTime, location, scheduledCount }.
 */
@Component({
  selector: 'app-session-edit-form',
  templateUrl: './session-edit-form.component.html',
  styleUrls: ['./session-edit-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class SessionEditFormComponent extends TdBaseComponent implements OnInit {
  params: {
    groupKey: string; className?: string | null;
    scheduledDate: string; startTime: string; endTime: string;
    location?: string | null; scheduledCount: number;
  } | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(LessonService);

  frmGroup!: FormGroup;
  isSubmitting = false;

  ngOnInit() {
    const p = this.params!;
    this.frmGroup = this._fb.group({
      scheduledDate: [new Date(p.scheduledDate), Validators.required],
      startTime: [this.parseTime(p.startTime), Validators.required],
      endTime: [this.parseTime(p.endTime), Validators.required],
      location: [p.location ?? ''],
    });
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    this.isSubmitting = true;
    this._service.updateGroup({
      groupKey: this.params!.groupKey,
      scheduledDate: this.toISODate(v.scheduledDate),
      startTime: this.formatTime(v.startTime),
      endTime: this.formatTime(v.endTime),
      location: v.location || '',
    }).pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS,
              `Đã đổi lịch cả ca (${rs.data} buổi) — nhắc phụ huynh đã cập nhật theo giờ mới`);
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
  private formatTime(d: Date): string {
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
  }
  private parseTime(t: string): Date {
    const [h, m] = (t || '00:00:00').split(':').map(n => parseInt(n, 10));
    const d = new Date(); d.setHours(h || 0, m || 0, 0, 0);
    return d;
  }
}
