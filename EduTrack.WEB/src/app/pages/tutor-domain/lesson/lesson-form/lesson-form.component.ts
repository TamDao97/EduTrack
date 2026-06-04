import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ILesson } from '../../../../interfaces/ILesson';
import { IStudentCourse } from '../../../../interfaces/IStudentCourse';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { LessonService } from '../../../../services/tutor-domain/lesson.service';
import { StudentCourseService } from '../../../../services/tutor-domain/student-course.service';
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
  params: ILesson | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(LessonService);
  private _studentService = inject(StudentService);
  private _courseService = inject(StudentCourseService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;
  students: IStudentDetail[] = [];
  /** Môn (đang active) của HS đang chọn — hiện select khi HS có ≥2 môn. */
  courses: IStudentCourse[] = [];

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
      this.loadCourses(this.params.idStudent, this.params.idCourse ?? null);
    }
  }

  initForm() {
    const now = new Date();
    const defaultStart = new Date(); defaultStart.setHours(19, 0, 0, 0);
    const defaultEnd = new Date(); defaultEnd.setHours(20, 30, 0, 0);
    this.frmGroup = this._fb.group({
      id: [null],
      idStudent: [null, [Validators.required]],
      idCourse: [null],
      scheduledDate: [now, [Validators.required]],
      startTime: [defaultStart, [Validators.required]],
      endTime: [defaultEnd, [Validators.required]],
      location: [''],
      notes: [''],
    });

    // Đổi HS → nạp môn của HS đó
    this.frmGroup.get('idStudent')!.valueChanges.subscribe((id: string) => {
      if (id) this.loadCourses(id, null);
      else { this.courses = []; this.frmGroup.patchValue({ idCourse: null }, { emitEvent: false }); }
    });
  }

  /** Nạp môn của HS; auto chọn nếu chỉ có 1 môn. keepCourse = giữ môn khi đang sửa buổi. */
  loadCourses(idStudent: string, keepCourse: string | null) {
    this._courseService.getByStudent(idStudent).subscribe((rs) => {
      if (rs.status !== StatusCode.Ok) return;
      this.courses = (rs.data ?? []).filter((c: IStudentCourse) => c.isActive);
      const current = keepCourse && this.courses.some(c => c.id === keepCourse) ? keepCourse
                    : this.courses.length > 0 ? this.courses[0].id : null;
      this.frmGroup.patchValue({ idCourse: current }, { emitEvent: false });
    });
  }

  /** Giá buổi của môn đang chọn — hiển thị cho tutor biết trước. */
  get selectedCourseRate(): number | null {
    const id = this.frmGroup?.value?.idCourse;
    return this.courses.find(c => c.id === id)?.perLessonRate ?? null;
  }

  fmtVnd(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

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
