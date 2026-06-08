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
  /** Id các đợt thu ĐÃ bị hoàn tác — để ẩn nút hoàn + gắn badge. */
  reversedIds = new Set<string>();
  /** Đã có thay đổi (hoàn tác) trong phiên modal — đóng kiểu gì list ngoài cũng phải reload. */
  private changed = false;

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
        if (rs.status === StatusCode.Ok) {
          this.payments = rs.data ?? [];
          this.reversedIds = new Set(
            this.payments.filter(p => p.idReversalOf).map(p => p.idReversalOf!)
          );
        }
      });
  }

  /** Đợt thu thường, chưa bị hoàn → hiện nút Hoàn tác. */
  canReverse(p: ITuitionPayment): boolean {
    return p.amount > 0 && !!p.id && !this.reversedIds.has(p.id);
  }

  isReversed(p: ITuitionPayment): boolean {
    return !!p.id && this.reversedIds.has(p.id);
  }

  /** Hoàn tác 1 đợt thu (ghi nhầm người/nhầm số) — confirm vì là hành động sổ sách. */
  onReverse(p: ITuitionPayment) {
    this.confirmModal(
      `Hoàn tác đợt thu ${this.fmt(p.amount)}đ ngày ${this.fmtDate(p.dateCreated)}? Kỳ sẽ trở về trạng thái chờ thu phần này.`,
      () => {
        this._service.reversePayment(p.id!, `Hoàn tác đợt thu ${this.fmtDate(p.dateCreated)} bởi gia sư`)
          .subscribe({
            next: rs => {
              if (rs.status === StatusCode.Ok) {
                this._toast.success(StatusResponseTitle.SUCCESS, 'Đã hoàn tác đợt thu');
                this.changed = true;
                this.applyPeriodUpdate(rs.data);
                this.loadHistory();
              } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
            },
            error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
          });
      }
    );
  }

  /** Đồng bộ lại summary + ô số tiền theo kỳ vừa được BE cập nhật sau hoàn tác. */
  private applyPeriodUpdate(period: { paidAmount: number; status: number } | null) {
    if (!this.params || !period) return;
    this.params.paidAmount = period.paidAmount;
    this.params.status = period.status;
    this.params.outstandingAmount = this.params.finalAmount - period.paidAmount;
    this.frmGroup.patchValue({ amount: this.outstanding });
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

  // Đã hoàn tác gì đó thì dù bấm Huỷ, list ngoài vẫn phải reload (saved=true)
  onCancel() { this.closeModal(this.changed ? { saved: true } : undefined); }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
}
