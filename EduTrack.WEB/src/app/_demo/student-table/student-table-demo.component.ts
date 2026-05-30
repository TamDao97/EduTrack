import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

type StudentStatus = 'active' | 'paused' | 'stopped';

interface StudentRow {
  selected?: boolean;
  fullName: string;
  grade: string;
  subject: string;
  parentName: string;
  parentPhone: string;
  rate: number;
  status: StudentStatus;
  startedAt: string;
}

@Component({
  selector: 'app-student-table-demo',
  templateUrl: './student-table-demo.component.html',
  styleUrls: ['./student-table-demo.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class StudentTableDemoComponent extends TdBaseComponent {
  private _router = inject(Router);

  // ─── Filter state ───
  keyword = '';
  statusFilter: StudentStatus | '' = '';
  parentFilter = '';

  // ─── View toggle (Card / Table) ───
  viewMode: 'card' | 'table' = 'table';

  rows: StudentRow[] = [
    { fullName: 'Nguyễn Minh Mai',  grade: 'Lớp 12', subject: 'Toán', parentName: 'Mẹ Mai',    parentPhone: '0901234567', rate: 200000, status: 'active',  startedAt: '15/01/2026' },
    { fullName: 'Trần Đức Nam',     grade: 'Lớp 11', subject: 'Lý',   parentName: 'Anh Nam',   parentPhone: '0912345678', rate: 180000, status: 'active',  startedAt: '03/02/2026' },
    { fullName: 'Lê Phương Hoa',    grade: 'Lớp 10', subject: 'Anh',  parentName: 'Mẹ Hoa',    parentPhone: '0987654321', rate: 150000, status: 'active',  startedAt: '20/02/2026' },
    { fullName: 'Phạm Bảo Anh',     grade: 'Lớp 9',  subject: 'Toán', parentName: 'Chị Anh',   parentPhone: '0934567890', rate: 150000, status: 'active',  startedAt: '01/03/2026' },
    { fullName: 'Vũ Hoàng Long',    grade: 'ĐH',     subject: 'Toán', parentName: 'Bố Long',   parentPhone: '0945678901', rate: 250000, status: 'active',  startedAt: '10/03/2026' },
    { fullName: 'Đặng Thuỳ Linh',   grade: 'Lớp 12', subject: 'Lý',   parentName: 'Mẹ Linh',   parentPhone: '0956789012', rate: 200000, status: 'paused',  startedAt: '20/12/2025' },
    { fullName: 'Bùi Quang Hùng',   grade: 'Lớp 11', subject: 'Hoá',  parentName: 'Bố Hùng',   parentPhone: '0967890123', rate: 180000, status: 'active',  startedAt: '15/03/2026' },
    { fullName: 'Hoàng Thanh Tâm',  grade: 'Lớp 12', subject: 'Văn',  parentName: 'Bà Tâm',    parentPhone: '0978901234', rate: 200000, status: 'stopped', startedAt: '01/09/2025' },
  ];

  get filtered(): StudentRow[] {
    return this.rows.filter(r => {
      if (this.statusFilter && r.status !== this.statusFilter) return false;
      if (this.parentFilter && !r.parentName.toLowerCase().includes(this.parentFilter.toLowerCase())) return false;
      if (this.keyword) {
        const kw = this.keyword.toLowerCase();
        return r.fullName.toLowerCase().includes(kw)
          || r.parentName.toLowerCase().includes(kw)
          || r.parentPhone.includes(kw)
          || r.subject.toLowerCase().includes(kw);
      }
      return true;
    });
  }

  get selectedCount(): number { return this.rows.filter(r => r.selected).length; }
  get allSelected(): boolean  { return this.filtered.length > 0 && this.filtered.every(r => r.selected); }

  toggleAll(checked: boolean) { this.filtered.forEach(r => r.selected = checked); }

  statusLabel(s: StudentStatus): string {
    return { active: 'Đang học', paused: 'Tạm nghỉ', stopped: 'Đã dừng' }[s];
  }
  statusClass(s: StudentStatus): string { return `tag-${s}`; }

  formatPhone(p: string): string {
    const d = p.replace(/\D/g, '');
    return d.length === 10 ? `${d.slice(0,4)} ${d.slice(4,7)} ${d.slice(7)}` : p;
  }

  onClose() { this._router.navigate(['/design']); }
}
