import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { finalize } from 'rxjs';
import { IClassRoom } from '../../../../interfaces/IClassRoom';
import { ClassRoomService } from '../../../../services/tutor-domain/class-room.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/** Modal tạo/sửa Lớp học — params: { classRoom? }. */
@Component({
  selector: 'app-class-form',
  templateUrl: './class-form.component.html',
  styleUrls: ['./class-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ClassFormComponent extends TdBaseComponent implements OnInit {
  params: { classRoom?: IClassRoom } | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(ClassRoomService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;
  /** Thứ đang chọn — .NET DayOfWeek (0=CN..6=T7), khớp JS getDay() */
  selectedDays: number[] = [];

  weekDays = [
    { value: 1, label: 'T2' }, { value: 2, label: 'T3' }, { value: 3, label: 'T4' },
    { value: 4, label: 'T5' }, { value: 5, label: 'T6' }, { value: 6, label: 'T7' },
    { value: 0, label: 'CN' },
  ];

  ngOnInit() {
    const start = new Date(); start.setHours(19, 0, 0, 0);
    const end = new Date(); end.setHours(20, 30, 0, 0);
    this.frmGroup = this._fb.group({
      id: [null],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      subject: [''],
      defaultRatePerLesson: [0, [Validators.required, Validators.min(0)]],
      startTime: [start, Validators.required],
      endTime: [end, Validators.required],
      location: [''],
      isActive: [true],
      notes: [''],
    });

    const c = this.params?.classRoom;
    if (c) {
      this.isEdit = true;
      this.frmGroup.patchValue({
        ...c,
        startTime: this.parseTime(c.startTime),
        endTime: this.parseTime(c.endTime),
      });
      this.selectedDays = (c.daysOfWeek || '').split(',')
        .map(s => parseInt(s, 10)).filter(n => !isNaN(n));
    }
  }

  toggleDay(d: number) {
    const i = this.selectedDays.indexOf(d);
    if (i >= 0) this.selectedDays.splice(i, 1); else this.selectedDays.push(d);
  }
  isDayOn(d: number): boolean { return this.selectedDays.includes(d); }

  onSave() {
    if (this.selectedDays.length === 0) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Chọn ít nhất 1 thứ trong tuần');
      return;
    }
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    const payload: IClassRoom = {
      ...v,
      daysOfWeek: this.selectedDays.join(','),
      startTime: this.formatTime(v.startTime),
      endTime: this.formatTime(v.endTime),
      defaultRatePerLesson: +v.defaultRatePerLesson || 0,
    };
    this.isSubmitting = true;
    const obs$ = this.isEdit ? this._service.update(payload) : this._service.create(payload);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS,
              this.isEdit ? StatusResponseMessage.UPDATE_SUCCESS : 'Đã tạo lớp — ghi danh HS rồi xếp lịch nhé');
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  formatVnd = (v: number): string => v == null ? '' : String(v).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  parseVnd = (v: string): string => v.replace(/\./g, '');

  private parseTime(t: string): Date {
    const [h, m] = (t || '00:00:00').split(':').map(n => parseInt(n, 10));
    const d = new Date(); d.setHours(h || 0, m || 0, 0, 0);
    return d;
  }
  private formatTime(d: Date): string {
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
  }
}
