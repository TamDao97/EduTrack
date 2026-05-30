import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import { ITuitionPeriodDetail, ITuitionPeriodGridFilter, TuitionStatus, TuitionStatusLabel } from '../../../interfaces/ITuitionPeriod';
import { TuitionPeriodService } from '../../../services/tutor-domain/tuition-period.service';
import { TutorProfileService } from '../../../services/tutor-domain/tutor-profile.service';
import { defaultGridFilter } from '../../../shared/interfaces/IBase-ext';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { buildTransferContent, buildVietQrUrl } from '../../../shared/utils/vietqr';
import { TuitionCloseFormComponent } from './tuition-close-form/tuition-close-form.component';
import { TuitionPaymentFormComponent } from './tuition-payment-form/tuition-payment-form.component';

@Component({
  selector: 'app-tuition-list',
  templateUrl: './tuition-list.component.html',
  styleUrls: ['./tuition-list.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class TuitionListComponent extends TdBaseComponent implements OnInit {
  TuitionStatus = TuitionStatus;
  TuitionStatusLabel = TuitionStatusLabel;

  private _service = inject(TuitionPeriodService);
  private _profileService = inject(TutorProfileService);
  private _toast = inject(ToastService);

  periods: ITuitionPeriodDetail[] = [];
  totalRecord = 0;
  isLoading = false;
  bankInfo: { bankName?: string; accountNumber?: string; accountHolder?: string } = {};
  filter: ITuitionPeriodGridFilter = { ...defaultGridFilter(), pageSize: 50, status: null };

  statusTabs: { value: TuitionStatus | null; label: string; dot?: string }[] = [
    { value: null, label: 'Tất cả' },
    { value: TuitionStatus.Closed,      label: 'Cần thu',    dot: '#F5222D' },
    { value: TuitionStatus.PartialPaid, label: 'Thu 1 phần', dot: '#FAAD14' },
    { value: TuitionStatus.Paid,        label: 'Đã thu',     dot: '#52C41A' },
    { value: TuitionStatus.Open,        label: 'Đang mở',    dot: '#8A91A8' },
  ];

  ngOnInit() {
    this.isLoading = true;
    forkJoin({
      profile: this._profileService.getMyProfile(),
      periods: this._service.gridLoadData(this.filter),
    }).pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: ({ profile, periods }) => {
          if (profile?.status === StatusCode.Ok) {
            this.bankInfo = {
              bankName:      profile.data?.bankName,
              accountNumber: profile.data?.bankAccountNumber,
              accountHolder: profile.data?.bankAccountHolder,
            };
          }
          if (periods?.status === StatusCode.Ok) {
            this.periods = periods.data?.data ?? [];
            this.totalRecord = periods.data?.totalRecord ?? 0;
          } else if (periods) {
            this._toast.error(StatusResponseTitle.ERROR, periods.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được dữ liệu'),
      });
  }

  reload() {
    this.isLoading = true;
    this._service.gridLoadData(this.filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) {
          this.periods = rs.data?.data ?? [];
          this.totalRecord = rs.data?.totalRecord ?? 0;
        }
      });
  }

  onSelectStatus(value: TuitionStatus | null) {
    this.filter.status = value;
    this.filter.pageNumber = 1;
    this.reload();
  }

  onClose(period?: ITuitionPeriodDetail) {
    this.openModal(
      { title: period ? 'Chốt lại kỳ' : 'Chốt kỳ học phí', width: 600, className: 'sheet-bottom-mobile' },
      TuitionCloseFormComponent,
      { params: period }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.reload(); });
  }

  onPayment(period: ITuitionPeriodDetail) {
    this.openModal(
      { title: 'Ghi nhận thanh toán', width: 480, className: 'sheet-bottom-mobile' },
      TuitionPaymentFormComponent,
      { params: period }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.reload(); });
  }

  /** Gửi nhắc nợ Zalo cho phụ huynh (mở deeplink với text soạn sẵn) */
  onRemindZalo(p: ITuitionPeriodDetail) {
    if (!p.parentPhone) {
      this._toast.warning(StatusResponseTitle.WARNING, 'HS chưa có SĐT phụ huynh');
      return;
    }
    const outstanding = p.outstandingAmount ?? (p.finalAmount - p.paidAmount);
    const accLine = this.bankInfo.bankName && this.bankInfo.accountNumber
      ? `\n• Ngân hàng: ${this.bankInfo.bankName}\n• Số TK: ${this.bankInfo.accountNumber}\n• Chủ TK: ${this.bankInfo.accountHolder || ''}\n• Nội dung: ${buildTransferContent(p.studentFullName || '', p.periodMonth, p.periodYear)}`
      : '';
    const text =
`Em chào phụ huynh ${p.parentFullName || ''}!
Em gửi học phí tháng ${String(p.periodMonth).padStart(2, '0')}/${p.periodYear} của ${p.studentFullName}:
• Số buổi đã dạy: ${p.totalLessons}
• Học phí: ${this.fmt(p.totalAmount)}đ${p.adjustment ? `\n• Điều chỉnh: ${this.fmt(p.adjustment)}đ` : ''}
• Tổng: ${this.fmt(p.finalAmount)}đ${p.paidAmount ? `\n• Đã đóng: ${this.fmt(p.paidAmount)}đ\n• Còn nợ: ${this.fmt(outstanding)}đ` : ''}${accLine}

Em cám ơn phụ huynh!`;

    const digits = p.parentPhone.replace(/\D/g, '');
    window.open(`https://zalo.me/${digits}?text=${encodeURIComponent(text)}`, '_blank');
  }

  /** URL ảnh VietQR cho period (null nếu thiếu thông tin) */
  qrUrl(p: ITuitionPeriodDetail): string | null {
    if (!this.bankInfo.bankName || !this.bankInfo.accountNumber) return null;
    const outstanding = p.outstandingAmount ?? (p.finalAmount - p.paidAmount);
    if (outstanding <= 0) return null;
    return buildVietQrUrl({
      bankName: this.bankInfo.bankName,
      accountNumber: this.bankInfo.accountNumber,
      accountHolder: this.bankInfo.accountHolder,
      amount: outstanding,
      addInfo: buildTransferContent(p.studentFullName || '', p.periodMonth, p.periodYear),
    });
  }

  outstandingOf(p: ITuitionPeriodDetail): number {
    return p.outstandingAmount ?? (p.finalAmount - p.paidAmount);
  }

  paidPercentOf(p: ITuitionPeriodDetail): number {
    if (!p.finalAmount) return 0;
    return Math.round((p.paidAmount / p.finalAmount) * 100);
  }

  countFor(s: TuitionStatus | null): number {
    if (s === null) return this.periods.length;
    return this.periods.filter(p => p.status === s).length;
  }

  /** Tổng cần thu (cả Closed + PartialPaid) */
  get totalOutstanding(): number {
    return this.periods
      .filter(p => p.status === TuitionStatus.Closed || p.status === TuitionStatus.PartialPaid)
      .reduce((sum, p) => sum + this.outstandingOf(p), 0);
  }

  /** Tổng đã thu (cả thời gian) */
  get totalCollected(): number {
    return this.periods.reduce((sum, p) => sum + (p.paidAmount || 0), 0);
  }

  statusLabel(s: TuitionStatus): string { return TuitionStatusLabel[s]; }
  statusColor(s: TuitionStatus): string {
    return ({ [TuitionStatus.Open]: '#8A91A8', [TuitionStatus.Closed]: '#F5222D',
             [TuitionStatus.PartialPaid]: '#FAAD14', [TuitionStatus.Paid]: '#52C41A' } as any)[s];
  }
  statusSoft(s: TuitionStatus): string {
    return ({ [TuitionStatus.Open]: '#EEF0F8', [TuitionStatus.Closed]: '#FFF1F0',
             [TuitionStatus.PartialPaid]: '#FFF7E0', [TuitionStatus.Paid]: '#F0FBE5' } as any)[s];
  }

  initials(name?: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    return (parts[0].charAt(0) + (parts.length > 1 ? parts[parts.length - 1].charAt(0) : '')).toUpperCase();
  }

  /** Avatar gradient theo hash tên */
  avatarStyle(name?: string): { [k: string]: string } {
    const palette = [
      ['#FFA1BD', '#FF6B9D'], ['#7C7FE0', '#5B5FCF'], ['#4ECDC4', '#00B8A9'],
      ['#FFB199', '#FF8A65'], ['#A78BFA', '#7C3AED'], ['#7DD3FC', '#0EA5E9'],
      ['#FCD34D', '#F59E0B'], ['#6EE7B7', '#10B981'],
    ];
    let h = 0;
    for (let i = 0; i < (name || '').length; i++) h = ((h << 5) - h) + (name || '').charCodeAt(i);
    const [a, b] = palette[Math.abs(h) % palette.length];
    return { background: `linear-gradient(135deg, ${a} 0%, ${b} 100%)` };
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  trackById(_: number, p: ITuitionPeriodDetail) { return p.id; }
}
