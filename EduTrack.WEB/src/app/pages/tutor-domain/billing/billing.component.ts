import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import { PlanCode, PlanLabel, PlanPrice, StatusLabel, SubscriptionStatus } from '../../../interfaces/IAdmin';
import { IMyPayment, IMySubscription } from '../../../interfaces/IBilling';
import { BillingService } from '../../../services/tutor-domain/billing.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { AuthService } from '../../../shared/utils/services/auth.service';
import { UpgradeFormComponent } from './upgrade-form/upgrade-form.component';

/**
 * Trang `/billing` cho tutor xem & nâng cấp gói. Khác /admin (founder).
 * - Hero: trạng thái plan hiện tại, ngày còn lại
 * - 2 plan cards (Basic, Pro) — current plan có badge
 * - Bảng lịch sử thanh toán
 */
@Component({
  selector: 'app-billing',
  templateUrl: './billing.component.html',
  styleUrls: ['./billing.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class BillingComponent extends TdBaseComponent implements OnInit {
  PlanCode = PlanCode;
  PlanLabel = PlanLabel;
  PlanPrice = PlanPrice;
  SubscriptionStatus = SubscriptionStatus;
  StatusLabel = StatusLabel;

  private _service = inject(BillingService);
  private _toast = inject(ToastService);

  sub: IMySubscription | null = null;
  payments: IMyPayment[] = [];
  isLoading = false;

  tutorDisplayName = '';
  tutorUserName = '';

  ngOnInit() {
    const authStr = AuthService.getAuthStorage();
    if (authStr) {
      try {
        const u = JSON.parse(authStr);
        this.tutorDisplayName = u.displayName || 'Gia sư';
        this.tutorUserName = u.userName || u.email || '';
      } catch {}
    }
    this.loadAll();
  }

  loadAll() {
    this.isLoading = true;
    forkJoin({
      sub: this._service.getMine(),
      payments: this._service.getMyPayments(),
    }).pipe(finalize(() => this.isLoading = false))
      .subscribe(({ sub, payments }) => {
        if (sub?.status === StatusCode.Ok) this.sub = sub.data;
        if (payments?.status === StatusCode.Ok) this.payments = payments.data ?? [];
      });
  }

  /** Click nút "Nâng cấp" → mở modal QR + bank info. */
  onUpgrade(plan: PlanCode) {
    this.openModal(
      { title: `Nâng cấp gói ${PlanLabel[plan]}`, width: 600, className: 'sheet-bottom-mobile' },
      UpgradeFormComponent,
      { params: { plan, displayName: this.tutorDisplayName, userName: this.tutorUserName } }
    );
  }

  /** UI helpers */
  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  fmtDate(s?: string): string {
    if (!s) return '—';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }

  get statusBadgeClass(): string {
    if (!this.sub) return '';
    return ({
      [SubscriptionStatus.Trial]: 'bd-trial',
      [SubscriptionStatus.Active]: 'bd-active',
      [SubscriptionStatus.Expired]: 'bd-expired',
      [SubscriptionStatus.Cancelled]: 'bd-cancelled',
    } as any)[this.sub.status];
  }

  /** "3 ngày" / "Hết hạn" / "12 ngày dùng thử" */
  get statusHeadline(): string {
    if (!this.sub) return '';
    const days = this.sub.daysRemaining ?? 0;
    if (this.sub.status === SubscriptionStatus.Expired) return 'Đã hết hạn';
    if (this.sub.status === SubscriptionStatus.Trial) {
      if (days <= 0) return 'Trial đã hết hạn';
      return `Còn ${days} ngày dùng thử`;
    }
    if (this.sub.status === SubscriptionStatus.Active) {
      if (days <= 0) return 'Đã hết hạn';
      return `Còn ${days} ngày`;
    }
    return StatusLabel[this.sub.status];
  }

  get statusUrgent(): boolean {
    if (!this.sub) return false;
    const days = this.sub.daysRemaining ?? 0;
    return this.sub.status === SubscriptionStatus.Expired || days <= 3;
  }

  planClass(p: PlanCode): string {
    return ({ [PlanCode.Free]: 'pl-free', [PlanCode.Basic]: 'pl-basic', [PlanCode.Pro]: 'pl-pro' } as any)[p];
  }

  isCurrentPlan(p: PlanCode): boolean {
    return !!this.sub && this.sub.plan === p
        && (this.sub.status === SubscriptionStatus.Active);
  }
}
