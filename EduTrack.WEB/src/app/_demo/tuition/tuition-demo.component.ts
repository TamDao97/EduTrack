import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

type TuitionStatus = 'open' | 'closed' | 'partial' | 'paid';

interface Period {
  studentName: string;
  avatarFrom: string;
  avatarTo: string;
  month: number;
  year: number;
  totalLessons: number;
  finalAmount: number;
  paidAmount: number;
  status: TuitionStatus;
  closedAt?: string;
  parentPhone?: string;
}

@Component({
  selector: 'app-tuition-demo',
  templateUrl: './tuition-demo.component.html',
  styleUrls: ['./tuition-demo.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class TuitionDemoComponent extends TdBaseComponent {
  private _router = inject(Router);

  periods: Period[] = [
    { studentName: 'Nguyễn Minh Mai', avatarFrom: '#FFA1BD', avatarTo: '#FF6B9D', month: 5, year: 2026, totalLessons: 10, finalAmount: 1500000, paidAmount: 0, status: 'closed', closedAt: '01/06/2026', parentPhone: '0901234567' },
    { studentName: 'Trần Đức Nam', avatarFrom: '#7C7FE0', avatarTo: '#5B5FCF', month: 5, year: 2026, totalLessons: 8, finalAmount: 1200000, paidAmount: 600000, status: 'partial', closedAt: '01/06/2026', parentPhone: '0912345678' },
    { studentName: 'Lê Phương Hoa', avatarFrom: '#4ECDC4', avatarTo: '#00B8A9', month: 5, year: 2026, totalLessons: 12, finalAmount: 1800000, paidAmount: 1800000, status: 'paid', closedAt: '01/06/2026', parentPhone: '0987654321' },
    { studentName: 'Phạm Bảo Anh', avatarFrom: '#FFB199', avatarTo: '#FF8A65', month: 5, year: 2026, totalLessons: 6, finalAmount: 900000, paidAmount: 0, status: 'open',  parentPhone: '0934567890' },
    { studentName: 'Vũ Hoàng Long', avatarFrom: '#FCD34D', avatarTo: '#F59E0B', month: 4, year: 2026, totalLessons: 9, finalAmount: 1350000, paidAmount: 0, status: 'closed', closedAt: '01/05/2026', parentPhone: '0945678901' },
  ];

  statusTabs: { value: TuitionStatus | null; label: string; dot?: string }[] = [
    { value: null, label: 'Tất cả' },
    { value: 'closed',  label: 'Cần thu',     dot: '#F5222D' },
    { value: 'partial', label: 'Thu 1 phần',  dot: '#FAAD14' },
    { value: 'paid',    label: 'Đã thu',      dot: '#52C41A' },
    { value: 'open',    label: 'Đang mở',     dot: '#8A91A8' },
  ];
  activeStatus: TuitionStatus | null = null;

  get filtered(): Period[] {
    return this.activeStatus ? this.periods.filter(p => p.status === this.activeStatus) : this.periods;
  }

  get totalOutstanding(): number {
    return this.periods
      .filter(p => p.status === 'closed' || p.status === 'partial')
      .reduce((sum, p) => sum + (p.finalAmount - p.paidAmount), 0);
  }
  get totalPaidThisMonth(): number {
    return this.periods.filter(p => p.month === 5 && p.year === 2026).reduce((sum, p) => sum + p.paidAmount, 0);
  }
  get totalPeriods(): number { return this.periods.length; }

  countFor(s: TuitionStatus | null): number {
    return s === null ? this.periods.length : this.periods.filter(p => p.status === s).length;
  }

  statusLabel(s: TuitionStatus): string {
    return { open: 'Đang mở', closed: 'Cần thu', partial: 'Thu 1 phần', paid: 'Đã thu' }[s];
  }
  statusColor(s: TuitionStatus): string {
    return { open: '#8A91A8', closed: '#F5222D', partial: '#FAAD14', paid: '#52C41A' }[s];
  }
  statusSoft(s: TuitionStatus): string {
    return { open: '#EEF0F8', closed: '#FFF1F0', partial: '#FFF7E0', paid: '#F0FBE5' }[s];
  }

  outstanding(p: Period): number { return p.finalAmount - p.paidAmount; }
  paidPercent(p: Period): number { return p.finalAmount ? Math.round((p.paidAmount / p.finalAmount) * 100) : 0; }

  initials(name: string): string {
    const parts = name.trim().split(/\s+/);
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  onClose() { this._router.navigate(['/design']); }
}
