import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzRateModule } from 'ng-zorro-antd/rate';
import { finalize } from 'rxjs';
import { FeedbackStatus, FeedbackStatusLabel, FeedbackType, FeedbackTypeLabel, IFeedback } from '../../../interfaces/IFeedback';
import { FeedbackService } from '../../../services/tutor-domain/feedback.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';

/**
 * Trang Góp ý — tutor gửi góp ý/báo lỗi/đề xuất cho founder
 * + xem trạng thái và phản hồi trên các góp ý đã gửi.
 */
@Component({
  selector: 'app-feedback',
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, NzRateModule],
})
export class FeedbackComponent extends TdBaseComponent implements OnInit {
  FeedbackType = FeedbackType;
  FeedbackTypeLabel = FeedbackTypeLabel;
  FeedbackStatus = FeedbackStatus;
  FeedbackStatusLabel = FeedbackStatusLabel;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(FeedbackService);

  frmGroup!: FormGroup;
  myFeedbacks: IFeedback[] = [];
  isLoading = false;
  isSubmitting = false;

  typeOptions = [
    { value: FeedbackType.GopY,     label: FeedbackTypeLabel[FeedbackType.GopY],     icon: 'message' },
    { value: FeedbackType.BaoLoi,   label: FeedbackTypeLabel[FeedbackType.BaoLoi],   icon: 'bug' },
    { value: FeedbackType.TinhNang, label: FeedbackTypeLabel[FeedbackType.TinhNang], icon: 'bulb' },
    { value: FeedbackType.Khac,     label: FeedbackTypeLabel[FeedbackType.Khac],     icon: 'ellipsis' },
  ];

  ngOnInit() {
    this.frmGroup = this._fb.group({
      type: [FeedbackType.GopY, Validators.required],
      rating: [null],
      title: ['', [Validators.required, Validators.maxLength(200)]],
      content: ['', Validators.required],
    });
    this.loadMine();
  }

  loadMine() {
    this.isLoading = true;
    this._service.getMine()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) this.myFeedbacks = rs.data ?? [];
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được danh sách góp ý'),
      });
  }

  selectType(t: FeedbackType) { this.frmGroup.patchValue({ type: t }); }

  onSubmit() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    this.isSubmitting = true;
    this._service.create(this.frmGroup.value)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, 'Cảm ơn bạn! Góp ý đã được gửi tới đội ngũ EduTrack.');
            this.frmGroup.reset({ type: FeedbackType.GopY, rating: null, title: '', content: '' });
            this.loadMine();
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống, vui lòng thử lại'),
      });
  }

  typeIcon(t: FeedbackType): string {
    return this.typeOptions.find(o => o.value === t)?.icon ?? 'message';
  }

  statusColor(s?: FeedbackStatus): string {
    return ({
      [FeedbackStatus.Moi]:        '#5B5FCF',
      [FeedbackStatus.DangXemXet]: '#FAAD14',
      [FeedbackStatus.SeLam]:      '#0EA5E9',
      [FeedbackStatus.DaLam]:      '#52C41A',
      [FeedbackStatus.TuChoi]:     '#9CA3AF',
    } as any)[s ?? FeedbackStatus.Moi];
  }

  statusSoft(s?: FeedbackStatus): string {
    return ({
      [FeedbackStatus.Moi]:        '#EEF0FF',
      [FeedbackStatus.DangXemXet]: '#FFF7E0',
      [FeedbackStatus.SeLam]:      '#E5F6FD',
      [FeedbackStatus.DaLam]:      '#F0FBE5',
      [FeedbackStatus.TuChoi]:     '#F1F2F4',
    } as any)[s ?? FeedbackStatus.Moi];
  }

  fmtDate(s?: string): string {
    if (!s) return '';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }

  trackById(_: number, f: IFeedback) { return f.id; }
}
