import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ILesson } from '../../../../interfaces/ILesson';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { LessonService } from '../../../../services/tutor-domain/lesson.service';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-lesson-form',
  templateUrl: './lesson-form.component.html',
  styleUrls: ['./lesson-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class LessonFormComponent extends TdBaseComponent implements OnInit {
  params: ILesson | null = null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(LessonService);
  private _studentService = inject(StudentService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;
  students: IStudentDetail[] = [];

  ngOnInit() {
    this.initForm();
    this.loadStudents();
    if (this.params) {
      this.isEdit = true;
      this.frmGroup.patchValue({
        ...this.params,
        scheduledDate: new Date(this.params.scheduledDate),
        startTime: this.parseTime(this.params.startTime),
        endTime: this.parseTime(this.params.endTime),
      });
    }
  }

  initForm() {
    const now = new Date();
    const defaultStart = new Date(); defaultStart.setHours(19, 0, 0, 0);
    const defaultEnd = new Date(); defaultEnd.setHours(20, 30, 0, 0);
    this.frmGroup = this._fb.group({
      id: [null],
      idStudent: [null, [Validators.required]],
      scheduledDate: [now, [Validators.required]],
      startTime: [defaultStart, [Validators.required]],
      endTime: [defaultEnd, [Validators.required]],
      location: [''],
      notes: [''],
    });
  }

  loadStudents() {
    this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200, status: 1 })
      .subscribe((rs) => {
        if (rs.status === StatusCode.Ok) this.students = rs.data?.data ?? [];
      });
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    const payload = {
      ...v,
      scheduledDate: this.toISODate(v.scheduledDate),
      startTime: this.formatTime(v.startTime),
      endTime: this.formatTime(v.endTime),
    };
    this.isSubmitting = true;
    const obs$ = this.isEdit ? this._service.update(payload) : this._service.create(payload);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, this.isEdit ? StatusResponseMessage.UPDATE_SUCCESS : StatusResponseMessage.ADD_SUCCESS);
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  /* helpers */
  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
  private formatTime(d: Date): string {
    if (!d) return '00:00:00';
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`;
  }
  private parseTime(t: string): Date {
    const [h, m] = (t || '00:00:00').split(':').map(n => parseInt(n, 10));
    const d = new Date(); d.setHours(h || 0, m || 0, 0, 0);
    return d;
  }
}
