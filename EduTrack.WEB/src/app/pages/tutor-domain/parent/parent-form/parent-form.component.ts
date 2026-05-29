import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { IParent } from '../../../../interfaces/IParent';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { ParentService } from '../../../../services/tutor-domain/parent.service';

/** Quick form thêm/sửa phụ huynh — 4 field cốt lõi. */
@Component({
  selector: 'app-parent-form',
  templateUrl: './parent-form.component.html',
  styleUrls: ['./parent-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ParentFormComponent extends TdBaseComponent implements OnInit {
  /** openModal truyền data qua đây. */
  params: IParent | null = null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(ParentService);

  frmGroup!: FormGroup;
  isEdit = false;
  isSubmitting = false;

  ngOnInit() {
    this.initForm();
    if (this.params) {
      this.isEdit = true;
      this.frmGroup.patchValue(this.params);
    }
  }

  initForm() {
    this.frmGroup = this._fb.group({
      id: [null],
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      phone: ['', [Validators.required, Validators.pattern(/^0\d{9,10}$/)]],
      email: [''],
      notes: [''],
    });
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const payload = { ...this.frmGroup.value };
    this.isSubmitting = true;
    const obs$ = this.isEdit ? this._service.update(payload) : this._service.create(payload);
    obs$.pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, this.isEdit ? StatusResponseMessage.UPDATE_SUCCESS : StatusResponseMessage.ADD_SUCCESS);
            this.closeModal({ parent: rs.data });
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống, vui lòng thử lại'),
      });
  }

  onCancel() { this.closeModal(); }
}
