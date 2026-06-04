import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { finalize } from 'rxjs';
import { IClassMember } from '../../../../interfaces/IClassRoom';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { ClassRoomService } from '../../../../services/tutor-domain/class-room.service';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

/**
 * Modal ghi danh HS vào lớp / sửa giá riêng.
 * params: { idClass, defaultRate, excludeIds, member? } — member = sửa giá riêng.
 */
@Component({
  selector: 'app-member-form',
  templateUrl: './member-form.component.html',
  styleUrls: ['./member-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class MemberFormComponent extends TdBaseComponent implements OnInit {
  params: { idClass: string; defaultRate: number; excludeIds?: string[]; member?: IClassMember } | null
    = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(ClassRoomService);
  private _studentService = inject(StudentService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;
  students: IStudentDetail[] = [];

  ngOnInit() {
    this.frmGroup = this._fb.group({
      idStudent: [null, Validators.required],
      useCustomRate: [false],
      rateOverride: [this.params?.defaultRate ?? 0],
    });

    if (this.params?.member) {
      this.isEdit = true;
      this.frmGroup.patchValue({
        idStudent: this.params.member.idStudent,
        useCustomRate: this.params.member.rateOverride != null,
        rateOverride: this.params.member.rateOverride ?? this.params.defaultRate,
      });
    } else {
      this.loadStudents();
    }
  }

  loadStudents() {
    this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200, status: 1 })
      .subscribe(rs => {
        if (rs.status !== StatusCode.Ok) return;
        const exclude = new Set(this.params?.excludeIds ?? []);
        this.students = (rs.data?.data ?? []).filter((s: IStudentDetail) => !exclude.has(s.id!));
      });
  }

  onSave() {
    if (!this.isEdit && !this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    const rate = v.useCustomRate ? (+v.rateOverride || 0) : null;

    this.isSubmitting = true;
    const obs$ = this.isEdit
      ? this._service.updateMember(this.params!.member!.id!, rate)
      : this._service.addMember(this.params!.idClass, v.idStudent, rate);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS,
              this.isEdit ? 'Đã cập nhật giá riêng' : 'Đã ghi danh vào lớp');
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  fmtVnd(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  formatVnd = (v: number): string => v == null ? '' : String(v).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  parseVnd = (v: string): string => v.replace(/\./g, '');
}
