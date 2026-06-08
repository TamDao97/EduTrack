import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { IMonthlyPoint, ISubjectRevenue, ITopStudent, ITutorReport } from '../../../interfaces/IReport';
import { ReportService } from '../../../services/tutor-domain/report.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';

type MetricKey = 'lessons' | 'revenue' | 'collected';
type RangeKey = '6m' | '12m' | 'ytd';

/**
 * Trang /report — báo cáo nhanh cho tutor:
 * - 4 stat boxes lifetime (KHÔNG đổi theo khoảng xem — thành tựu tích luỹ)
 * - Bar chart / doanh thu theo môn / top HS theo khoảng xem (6 tháng / 12 tháng / năm nay)
 * - Toggle metric chart: buổi / doanh thu / đã thu
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
  private _router = inject(Router);

  Math = Math;  // expose cho template
  report: ITutorReport | null = null;
  isLoading = false;
  metric: MetricKey = 'revenue';
  range: RangeKey = '6m';

  ngOnInit() {
    this.load();
  }

  load() {
    this.isLoading = true;
    this._service.getMyReport(this.rangeMonths)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) this.report = rs.data;
      });
  }

  setMetric(m: MetricKey) { this.metric = m; }

  /* ─── Khoảng xem: áp cho chart + theo môn + top HS (hero giữ lifetime) ─── */

  setRange(r: RangeKey) {
    if (this.range === r) return;
    this.range = r;
    this.load();
  }

  /** Số tháng gửi BE — "Năm nay" = từ tháng 1 đến tháng hiện tại. */
  private get rangeMonths(): number {
    if (this.range === '12m') return 12;
    if (this.range === 'ytd') return new Date().getMonth() + 1;
    return 6;
  }

  /** Tiêu đề khối chart theo khoảng xem. */
  get rangeTitle(): string {
    if (this.range === '12m') return '12 tháng gần nhất';
    if (this.range === 'ytd') return `Năm ${new Date().getFullYear()}`;
    return '6 tháng gần nhất';
  }

  /** Nhãn ngắn gắn vào legend / hint các khối. */
  get rangeShort(): string {
    if (this.range === '12m') return '12 tháng';
    if (this.range === 'ytd') return `năm ${new Date().getFullYear()}`;
    return '6 tháng';
  }

  /** Drill-down: bấm HS trong Top → trang chi tiết em đó. */
  goToStudent(s: ITopStudent) { this._router.navigate(['/student', s.idStudent]); }

  /** % chiều rộng bar môn — so với môn cao nhất. */
  subjectBarWidth(s: ISubjectRevenue): number {
    const list = this.report?.revenueBySubject ?? [];
    const max = Math.max(...list.map(x => x.amount), 1);
    return Math.max(4, Math.round((s.amount / max) * 100));
  }

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
