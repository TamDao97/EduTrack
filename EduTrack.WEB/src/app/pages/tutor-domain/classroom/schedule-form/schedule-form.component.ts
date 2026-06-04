import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { finalize } from 'rxjs';
import { ClassRoomService } from '../../../../services/tutor-domain/class-room.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/** Modal "Xếp lịch N tuần" cho lớp — params: { idClass, scheduleLabel }. */
@Component({
  selector: 'app-schedule-form',
  templateUrl: './schedule-form.component.html',
  styleUrls: ['./schedule-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ScheduleFormComponent extends TdBaseComponent implements OnInit {
  params: { idClass: string; scheduleLabel: string } | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(ClassRoomService);

  frmGroup!: FormGroup;
  isSubmitting = false;

  ngOnInit() {
    this.frmGroup = this._fb.group({
      startDate: [new Date(), Validators.required],
      numberOfWeeks: [4, [Validators.required, Validators.min(1), Validators.max(52)]],
    });
  }

  onSave() {
    if (!this.validateForm(this.frmGroup) || !this.params) return;
    const v = this.frmGroup.value;
    const d: Date = v.startDate;
    const iso = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    this.isSubmitting = true;
    this._service.generateSchedule(this.params.idClass, iso, +v.numberOfWeeks || 1)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            const n = rs.data ?? 0;
            this._toast.success(StatusResponseTitle.SUCCESS,
              n > 0 ? `Đã xếp ${n} buổi cho cả lớp (kèm nhắc tự động)` : 'Khoảng này đã xếp đủ — không có buổi mới');
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }
}
