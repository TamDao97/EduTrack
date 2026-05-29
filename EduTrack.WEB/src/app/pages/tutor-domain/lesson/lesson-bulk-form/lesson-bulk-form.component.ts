import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { LessonService } from '../../../../services/tutor-domain/lesson.service';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

interface WeekDayOption { value: number; label: string; }

@Component({
  selector: 'app-lesson-bulk-form',
  templateUrl: './lesson-bulk-form.component.html',
  styleUrls: ['./lesson-bulk-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class LessonBulkFormComponent extends TdBaseComponent implements OnInit {
  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(LessonService);
  private _studentService = inject(StudentService);

  frmGroup!: FormGroup;
  isSubmitting = false;
  students: IStudentDetail[] = [];

  /** value khớp JS Date.getDay() (Sun=0..Sat=6) */
  weekDays: WeekDayOption[] = [
    { value: 1, label: 'T2' },
    { value: 2, label: 'T3' },
    { value: 3, label: 'T4' },
    { value: 4, label: 'T5' },
    { value: 5, label: 'T6' },
    { value: 6, label: 'T7' },
    { value: 0, label: 'CN' },
  ];

  ngOnInit() {
    this.initForm();
    this.loadStudents();
  }

  initForm() {
    const start = new Date(); start.setHours(19, 0, 0, 0);
    const end = new Date(); end.setHours(20, 30, 0, 0);
    this.frmGroup = this._fb.group({
      idStudent: [null, [Validators.required]],
      startDate: [new Date(), [Validators.required]],
      numberOfWeeks: [12, [Validators.required, Validators.min(1), Validators.max(52)]],
      daysOfWeek: [[], [Validators.required]],
      startTime: [start, [Validators.required]],
      endTime: [end, [Validators.required]],
      location: [''],
    });
  }

  loadStudents() {
    this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200, status: 1 })
      .subscribe((rs) => {
        if (rs.status === StatusCode.Ok) this.students = rs.data?.data ?? [];
      });
  }

  toggleDay(d: number) {
    const current: number[] = [...(this.frmGroup.value.daysOfWeek || [])];
    const idx = current.indexOf(d);
    if (idx >= 0) current.splice(idx, 1); else current.push(d);
    this.frmGroup.patchValue({ daysOfWeek: current });
  }

  isDayOn(d: number): boolean {
    return (this.frmGroup.value.daysOfWeek || []).includes(d);
  }

  get totalLessons(): number {
    const days = this.frmGroup.value.daysOfWeek || [];
    const weeks = this.frmGroup.value.numberOfWeeks || 0;
    return days.length * weeks;
  }

  onSave() {
    if ((this.frmGroup.value.daysOfWeek || []).length === 0) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Chọn ít nhất 1 thứ trong tuần');
      return;
    }
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    const payload = {
      idStudent: v.idStudent,
      startDate: this.toISODate(v.startDate),
      numberOfWeeks: v.numberOfWeeks,
      daysOfWeek: v.daysOfWeek,
      startTime: this.formatTime(v.startTime),
      endTime: this.formatTime(v.endTime),
      location: v.location || '',
    };
    this.isSubmitting = true;
    this._service.bulkCreateRecurring(payload)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, `Đã tạo ${rs.data} buổi học`);
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
}
