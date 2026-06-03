import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ITutorWithSub, PlanCode, PlanLabel, PlanPrice } from '../../../interfaces/IAdmin';
import { AdminService } from '../../../services/admin/admin.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-confirm-payment-form',
  templateUrl: './confirm-payment-form.component.html',
  styleUrls: ['./confirm-payment-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ConfirmPaymentFormComponent extends TdBaseComponent implements OnInit {
  params: ITutorWithSub | null = inject(NZ_MODAL_DATA)?.params ?? null;
  PlanCode = PlanCode;
  PlanLabel = PlanLabel;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(AdminService);

  frmGroup!: FormGroup;
  isSubmitting = false;

  monthOptions = [1, 3, 6, 12];

  ngOnInit() {
    this.frmGroup = this._fb.group({
      plan: [this.params?.plan === PlanCode.Pro ? PlanCode.Pro : PlanCode.Basic, Validators.required],
      months: [1, [Validators.required, Validators.min(1), Validators.max(24)]],
      amount: [PlanPrice[PlanCode.Basic], [Validators.required, Validators.min(1)]],
      transferRef: [''],
      notes: [''],
    });

    // Auto-update amount khi đổi plan/months
    this.frmGroup.get('plan')!.valueChanges.subscribe(() => this.recalc());
    this.frmGroup.get('months')!.valueChanges.subscribe(() => this.recalc());
    this.recalc();
  }

  recalc() {
    const v = this.frmGroup.value;
    const price = PlanPrice[v.plan] || 0;
    this.frmGroup.patchValue({ amount: price * v.months }, { emitEvent: false });
  }

  setMonths(m: number) { this.frmGroup.patchValue({ months: m }); }

  onSave() {
    if (!this.validateForm(this.frmGroup) || !this.params) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    const v = this.frmGroup.value;
    this.isSubmitting = true;
    this._service.confirmPayment({
      idTutor: this.params.idTutor,
      plan: v.plan,
      months: v.months,
      amount: +v.amount,
      transferRef: v.transferRef,
      notes: v.notes,
    }).pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) this.closeModal({ saved: true });
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
}
