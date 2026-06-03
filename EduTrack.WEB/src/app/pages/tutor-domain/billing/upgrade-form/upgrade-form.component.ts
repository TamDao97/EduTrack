import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { environment } from '../../../../../environment';
import { PlanCode, PlanLabel, PlanPrice } from '../../../../interfaces/IAdmin';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { buildVietQrUrl } from '../../../../shared/utils/vietqr';

interface UpgradeFormParams {
  plan: PlanCode;
  displayName: string;
  userName: string;
}

/**
 * Modal upgrade plan — hiển thị VietQR + bank info + Zalo deeplink để tutor
 * chuyển tiền và báo founder. KHÔNG tự confirm — admin sẽ xác nhận trong /admin.
 */
@Component({
  selector: 'app-upgrade-form',
  templateUrl: './upgrade-form.component.html',
  styleUrls: ['./upgrade-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class UpgradeFormComponent extends TdBaseComponent implements OnInit {
  params: UpgradeFormParams | null = inject(NZ_MODAL_DATA)?.params ?? null;
  PlanCode = PlanCode;
  PlanLabel = PlanLabel;

  founder = environment.founder;
  selectedMonths = 1;
  monthOptions = [1, 3, 6, 12];
  qrUrl: string | null = null;
  transferContent = '';
  copyDone: 'acc' | 'amt' | 'note' | null = null;

  ngOnInit() {
    this.recalc();
  }

  setMonths(m: number) {
    this.selectedMonths = m;
    this.recalc();
  }

  get plan(): PlanCode { return this.params?.plan ?? PlanCode.Basic; }
  get pricePerMonth(): number { return PlanPrice[this.plan] || 0; }
  get totalAmount(): number { return this.pricePerMonth * this.selectedMonths; }

  recalc() {
    const tutor = this.params?.userName || 'tutor';
    const planTag = this.plan === PlanCode.Pro ? 'PRO' : 'BASIC';
    this.transferContent = `EDU ${planTag} ${this.selectedMonths}T ${tutor}`.toUpperCase();

    this.qrUrl = buildVietQrUrl({
      bankName: this.founder.bankName,
      accountNumber: this.founder.bankAccountNumber,
      accountHolder: this.founder.bankAccountHolder,
      amount: this.totalAmount,
      addInfo: this.transferContent,
      template: 'compact',
    });
  }

  copy(text: string, which: 'acc' | 'amt' | 'note') {
    navigator.clipboard?.writeText(text).then(() => {
      this.copyDone = which;
      setTimeout(() => (this.copyDone = null), 1500);
    });
  }

  /** Mở Zalo deeplink → tutor nhắn founder để xác nhận đã chuyển. */
  openZalo() {
    const phone = (this.founder.zaloPhone || '').replace(/\D/g, '');
    const msg = encodeURIComponent(
      `Chào shop, mình là ${this.params?.displayName} (${this.params?.userName}). ` +
      `Vừa chuyển ${new Intl.NumberFormat('vi-VN').format(this.totalAmount)}đ ` +
      `gói ${PlanLabel[this.plan]} ${this.selectedMonths} tháng. Nhờ shop xác nhận giúp ạ.`
    );
    // Zalo official deeplink: zalo.me/<phone>
    window.open(`https://zalo.me/${phone}?text=${msg}`, '_blank');
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  onClose() { this.closeModal(); }
}
