import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { IParent } from '../../../../interfaces/IParent';
import { IStudent, StudentStatus, StudentStatusLabel } from '../../../../interfaces/IStudent';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { ParentService } from '../../../../services/tutor-domain/parent.service';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { ParentFormComponent } from '../../parent/parent-form/parent-form.component';

@Component({
  selector: 'app-student-form',
  templateUrl: './student-form.component.html',
  styleUrls: ['./student-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class StudentFormComponent extends TdBaseComponent implements OnInit {
  StudentStatus = StudentStatus;
  StudentStatusLabel = StudentStatusLabel;

  /** openModal truyền data qua đây — đặt public để Partial<T> chấp nhận `{ params }`. */
  params: IStudent | null = null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _studentService = inject(StudentService);
  private _parentService = inject(ParentService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;
  parents: IParent[] = [];

  statusOptions = [
    { value: StudentStatus.Active, label: StudentStatusLabel[StudentStatus.Active] },
    { value: StudentStatus.Paused, label: StudentStatusLabel[StudentStatus.Paused] },
    { value: StudentStatus.Stopped, label: StudentStatusLabel[StudentStatus.Stopped] },
  ];

  ngOnInit() {
    this.initForm();
    this.loadParents();
    if (this.params) {
      this.isEdit = true;
      this.frmGroup.patchValue({
        ...this.params,
        dateBirth: this.params.dateBirth ? new Date(this.params.dateBirth) : null,
        startedAt: this.params.startedAt ? new Date(this.params.startedAt) : null,
      });
    }
  }

  initForm() {
    this.frmGroup = this._fb.group({
      id: [null],
      idParent: [null, [Validators.required]],
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      dateBirth: [null],
      grade: [''],
      subject: [''],
      perLessonRate: [0, [Validators.required, Validators.min(0)]],
      avatarFileId: [null],
      status: [StudentStatus.Active, [Validators.required]],
      startedAt: [new Date()],
      notes: [''],
    });
  }

  loadParents() {
    this._parentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200 })
      .subscribe((rs) => {
        if (rs.status === StatusCode.Ok) {
          this.parents = rs.data?.data ?? [];
        }
      });
  }

  /** Mở quick-form thêm phụ huynh mới → điền id vào form */
  onAddParent() {
    this.openModal(
      { title: 'Thêm phụ huynh', width: 480, className: 'sheet-bottom-mobile' },
      ParentFormComponent,
      {}
    ).afterClose.subscribe((rs: any) => {
      if (rs?.parent) {
        this.parents = [rs.parent, ...this.parents];
        this.frmGroup.patchValue({ idParent: rs.parent.id });
      }
    });
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const payload = { ...this.frmGroup.value };
    this.isSubmitting = true;
    const obs$ = this.isEdit ? this._studentService.update(payload) : this._studentService.create(payload);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, this.isEdit ? StatusResponseMessage.UPDATE_SUCCESS : StatusResponseMessage.ADD_SUCCESS);
            this.closeModal({ saved: true });
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống, vui lòng thử lại'),
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
