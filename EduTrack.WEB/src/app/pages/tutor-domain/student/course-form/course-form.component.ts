import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { finalize } from 'rxjs';
import { IStudentCourse } from '../../../../interfaces/IStudentCourse';
import { StudentCourseService } from '../../../../services/tutor-domain/student-course.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/** Modal thêm/sửa Môn học của 1 HS — params: { idStudent, course? }. */
@Component({
  selector: 'app-course-form',
  templateUrl: './course-form.component.html',
  styleUrls: ['./course-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class CourseFormComponent extends TdBaseComponent implements OnInit {
  params: { idStudent: string; course?: IStudentCourse } | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(StudentCourseService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;

  ngOnInit() {
    this.frmGroup = this._fb.group({
      id: [null],
      idStudent: [this.params?.idStudent, Validators.required],
      subject: ['', [Validators.required, Validators.maxLength(100)]],
      perLessonRate: [0, [Validators.required, Validators.min(0)]],
      isActive: [true],
      notes: [''],
    });
    if (this.params?.course) {
      this.isEdit = true;
      this.frmGroup.patchValue(this.params.course);
    }
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const payload = { ...this.frmGroup.value, perLessonRate: +this.frmGroup.value.perLessonRate || 0 };
    this.isSubmitting = true;
    const obs$ = this.isEdit ? this._service.update(payload) : this._service.create(payload);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS,
              this.isEdit ? StatusResponseMessage.UPDATE_SUCCESS : StatusResponseMessage.ADD_SUCCESS);
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  /** Format số tiền VN: 1500000 → "1.500.000" */
  formatVnd = (v: number): string => {
    if (v === null || v === undefined) return '';
    return String(v).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  };
  parseVnd = (v: string): string => v.replace(/\./g, '');
}
