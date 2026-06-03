import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IAdminPaymentRow, PlanCode, PlanLabel } from '../../interfaces/IAdmin';
import { AdminService } from '../../services/admin/admin.service';
import { defaultGridFilter } from '../../shared/interfaces/IBase-ext';
import { SharedModule } from '../../shared/modules/shared.module';
import { StatusCode } from '../../shared/utils/enums';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

/**
 * Lịch sử thanh toán subscription (tách từ tab cũ của admin-dashboard) — route /admin/payments.
 * Dùng lại style của admin-dashboard.component.scss để đồng bộ giao diện console.
 */
@Component({
  selector: 'app-admin-payments',
  templateUrl: './admin-payments.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class AdminPaymentsComponent extends TdBaseComponent implements OnInit {
  PlanLabel = PlanLabel;

  private _service = inject(AdminService);

  payments: IAdminPaymentRow[] = [];
  totalPayments = 0;
  isLoading = false;

  filter = { ...defaultGridFilter(), pageSize: 50 };

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.isLoading = true;
    this._service.getPayments(this.filter).subscribe((rs) => {
      this.isLoading = false;
      if (rs.status === StatusCode.Ok) {
        this.payments = rs.data?.data ?? [];
        this.totalPayments = rs.data?.totalRecord ?? 0;
      }
    });
  }

  onSearch(value: string): void {
    this.filter.keyword = value;
    this.filter.pageNumber = 1;
    this.loadPayments();
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  fmtDate(s?: string): string {
    if (!s) return '—';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }
  planClass(p: PlanCode): string {
    return ({ [PlanCode.Free]: 'tag-free', [PlanCode.Basic]: 'tag-basic', [PlanCode.Pro]: 'tag-pro' } as any)[p];
  }
}
