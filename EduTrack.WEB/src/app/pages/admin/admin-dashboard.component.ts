import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import {
  IAdminStats, IAdminTutorFilter, ITutorWithSub,
  PlanCode, PlanLabel, PlanPrice, StatusLabel, SubscriptionStatus,
} from '../../interfaces/IAdmin';
import { AdminService } from '../../services/admin/admin.service';
import { defaultGridFilter } from '../../shared/interfaces/IBase-ext';
import { SharedModule } from '../../shared/modules/shared.module';
import { ToastService } from '../../shared/services/toast.service';
import { StatusResponseTitle } from '../../shared/utils/constants';
import { StatusCode } from '../../shared/utils/enums';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';
import { ConfirmPaymentFormComponent } from './confirm-payment-form/confirm-payment-form.component';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class AdminDashboardComponent extends TdBaseComponent implements OnInit {
  // Expose enums cho template
  PlanCode = PlanCode;
  SubscriptionStatus = SubscriptionStatus;
  PlanLabel = PlanLabel;
  StatusLabel = StatusLabel;
  PlanPrice = PlanPrice;

  private _service = inject(AdminService);
  private _toast = inject(ToastService);

  stats: IAdminStats | null = null;
  tutors: ITutorWithSub[] = [];
  totalTutors = 0;
  isLoading = false;

  filter: IAdminTutorFilter = { ...defaultGridFilter(), pageSize: 50, plan: null, status: null };

  // Quyền truy cập (isSuper) đã được SuperGuard ở route cha /admin đảm bảo.
  ngOnInit() {
    this.loadAll();
  }

  loadAll() {
    this.isLoading = true;
    forkJoin({
      stats: this._service.getStats(),
      tutors: this._service.getTutors(this.filter),
    }).pipe(finalize(() => this.isLoading = false))
      .subscribe(({ stats, tutors }) => {
        if (stats?.status === StatusCode.Ok) this.stats = stats.data;
        if (tutors?.status === StatusCode.Ok) {
          this.tutors = tutors.data?.data ?? [];
          this.totalTutors = tutors.data?.totalRecord ?? 0;
        }
      });
  }

  reloadTutors() {
    this._service.getTutors(this.filter).subscribe(rs => {
      if (rs.status === StatusCode.Ok) {
        this.tutors = rs.data?.data ?? [];
        this.totalTutors = rs.data?.totalRecord ?? 0;
      }
    });
    this._service.getStats().subscribe(rs => {
      if (rs.status === StatusCode.Ok) this.stats = rs.data;
    });
  }

  onConfirmPayment(t: ITutorWithSub) {
    this.openModal(
      { title: `Xác nhận thanh toán — ${t.displayName}`, width: 520, className: 'sheet-bottom-mobile' },
      ConfirmPaymentFormComponent,
      { params: t }
    ).afterClose.subscribe(rs => {
      if (rs?.saved) {
        this._toast.success(StatusResponseTitle.SUCCESS, 'Đã ghi nhận + gia hạn subscription');
        this.reloadTutors();
      }
    });
  }

  /** Filter handlers */
  onSearch(value: string) {
    this.filter.keyword = value;
    this.filter.pageNumber = 1;
    this.reloadTutors();
  }
  onFilterPlan(plan: PlanCode | null) {
    this.filter.plan = plan;
    this.filter.pageNumber = 1;
    this.reloadTutors();
  }
  onFilterStatus(status: SubscriptionStatus | null) {
    this.filter.status = status;
    this.filter.pageNumber = 1;
    this.reloadTutors();
  }

  /** Helpers UI */
  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  fmtCompact(n: number): string {
    if (n >= 1000000) return (n / 1000000).toFixed(1).replace(/\.0$/, '') + 'tr';
    if (n >= 1000) return Math.round(n / 1000) + 'k';
    return String(n || 0);
  }
  fmtDate(s?: string): string {
    if (!s) return '—';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }

  statusClass(s: SubscriptionStatus): string {
    return ({ [SubscriptionStatus.Trial]: 'tag-trial',
              [SubscriptionStatus.Active]: 'tag-active',
              [SubscriptionStatus.Expired]: 'tag-expired',
              [SubscriptionStatus.Cancelled]: 'tag-cancelled' } as any)[s];
  }
  planClass(p: PlanCode): string {
    return ({ [PlanCode.Free]: 'tag-free',
              [PlanCode.Basic]: 'tag-basic',
              [PlanCode.Pro]: 'tag-pro' } as any)[p];
  }
}
