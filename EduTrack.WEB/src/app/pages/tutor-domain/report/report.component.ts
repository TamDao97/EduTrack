import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { IMonthlyPoint, ITutorReport } from '../../../interfaces/IReport';
import { ReportService } from '../../../services/tutor-domain/report.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';

type MetricKey = 'lessons' | 'revenue' | 'collected';

/**
 * Trang /report — báo cáo nhanh cho tutor:
 * - 4 stat boxes lifetime
 * - Bar chart 6 tháng (toggle metric: buổi / doanh thu / đã thu)
 * - Top 5 HS theo doanh thu
 */
@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class ReportComponent extends TdBaseComponent implements OnInit {
  private _service = inject(ReportService);

  Math = Math;  // expose cho template
  report: ITutorReport | null = null;
  isLoading = false;
  metric: MetricKey = 'revenue';

  ngOnInit() {
    this.load();
  }

  load() {
    this.isLoading = true;
    this._service.getMyReport()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) this.report = rs.data;
      });
  }

  setMetric(m: MetricKey) { this.metric = m; }

  valOf(p: IMonthlyPoint): number {
    if (this.metric === 'lessons') return p.lessonsDone;
    if (this.metric === 'collected') return p.collected;
    return p.revenue;
  }

  get maxValue(): number {
    if (!this.report?.months?.length) return 1;
    const max = Math.max(...this.report.months.map(p => this.valOf(p)));
    return max > 0 ? max : 1;
  }

  barHeight(p: IMonthlyPoint): number {
    return Math.round((this.valOf(p) / this.maxValue) * 100);
  }

  monthLabel(p: IMonthlyPoint): string {
    return `T${String(p.month).padStart(2, '0')}`;
  }

  isCurrentMonth(p: IMonthlyPoint): boolean {
    const n = new Date();
    return p.year === n.getFullYear() && p.month === n.getMonth() + 1;
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  fmtCompact(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'tr';
    if (n >= 1_000) return Math.round(n / 1_000) + 'k';
    return String(n || 0);
  }

  get metricLabel(): string {
    return this.metric === 'lessons' ? 'Số buổi'
         : this.metric === 'collected' ? 'Đã thu'
         : 'Doanh thu';
  }
}
