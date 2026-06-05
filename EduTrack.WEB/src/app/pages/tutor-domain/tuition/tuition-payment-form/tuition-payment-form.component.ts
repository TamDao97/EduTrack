import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ITuitionPayment, ITuitionPeriodDetail } from '../../../../interfaces/ITuitionPeriod';
import { TuitionPeriodService } from '../../../../services/tutor-domain/tuition-period.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-tuition-payment-form',
  templateUrl: './tuition-payment-form.component.html',
  styleUrls: ['./tuition-payment-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class TuitionPaymentFormComponent extends TdBaseComponent implements OnInit {
  params: ITuitionPeriodDetail | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(TuitionPeriodService);

  frmGroup!: FormGroup;
  isSubmitting = false;

  /** Lịch sử các đợt thu của kỳ — soi lại từng lần. */
  payments: ITuitionPayment[] = [];
  isLoadingHistory = false;

  methodOptions = ['Chuyển khoản', 'Tiền mặt', 'Khác'];

  ngOnInit() {
    const outstanding = this.outstanding;
    this.frmGroup = this._fb.group({
      amount: [outstanding, [Validators.required, Validators.min(1)]],
      method: ['Chuyển khoản'],
      notes: [''],
    });
    this.loadHistory();
  }

  loadHistory() {
    if (!this.params?.id) return;
    this.isLoadingHistory = true;
    this._service.getPayments(this.params.id)
      .pipe(finalize(() => this.isLoadingHistory = false))
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) this.payments = rs.data ?? [];
      });
  }

  fmtDate(s?: string): string {
    if (!s) return '';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }

  get outstanding(): number {
    if (!this.params) return 0;
    return this.params.outstandingAmount ?? (this.params.finalAmount - this.params.paidAmount);
  }

  fillFull() { this.frmGroup.patchValue({ amount: this.outstanding }); }

  onSave() {
    if (!this.params?.id) {
      this._toast.error(StatusResponseTitle.ERROR, 'Thiếu thông tin kỳ');
      return;
    }
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    if (+v.amount > this.outstanding) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Số tiền lớn hơn còn nợ');
      return;
    }
    this.isSubmitting = true;
    this._service.recordPayment(this.params.id, +v.amount, v.notes || '', v.method || null)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, 'Đã ghi nhận thanh toán');
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
}
