import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { IStudentDetail } from '../../../../interfaces/IStudent';
import { ITuitionPeriod, ITuitionPreview, TuitionStatus } from '../../../../interfaces/ITuitionPeriod';
import { StudentService } from '../../../../services/tutor-domain/student.service';
import { TuitionPeriodService } from '../../../../services/tutor-domain/tuition-period.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-tuition-close-form',
  templateUrl: './tuition-close-form.component.html',
  styleUrls: ['./tuition-close-form.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class TuitionCloseFormComponent extends TdBaseComponent implements OnInit {
  /** Optional — nếu mở từ 1 period đang Open có sẵn */
  params: ITuitionPeriod | null = inject(NZ_MODAL_DATA)?.params ?? null;

  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _service = inject(TuitionPeriodService);
  private _studentService = inject(StudentService);
  private _router = inject(Router);

  frmGroup!: FormGroup;
  students: IStudentDetail[] = [];
  preview: ITuitionPreview | null = null;
  currentPeriodId: string | null = null;
  isLoadingPreview = false;
  isSubmitting = false;

  /** Tháng/Năm dropdown — list 6 tháng gần đây (current - 0..5) */
  monthOptions: { value: number; label: string; month: number; year: number }[] = [];

  ngOnInit() {
    this.initForm();
    this.loadStudents();
    this.buildMonthOptions();

    if (this.params) {
      // Edit/close existing period
      this.frmGroup.patchValue({
        idStudent: this.params.idStudent,
        monthYear: this.params.periodYear * 100 + this.params.periodMonth,
        adjustment: this.params.adjustment || 0,
        notes: this.params.notes || '',
      });
      this.currentPeriodId = this.params.id || null;
      this.fetchPreviewFromForm();
    } else {
      // New — preset tháng trước (vừa qua)
      const d = new Date();
      d.setMonth(d.getMonth() - 1);
      this.frmGroup.patchValue({ monthYear: d.getFullYear() * 100 + (d.getMonth() + 1) });
    }
  }

  initForm() {
    this.frmGroup = this._fb.group({
      idStudent: [null, Validators.required],
      monthYear: [null, Validators.required],
      adjustment: [0],
      notes: [''],
    });

    // Khi đổi HS hoặc tháng → fetch preview mới
    this.frmGroup.get('idStudent')!.valueChanges.subscribe(() => this.fetchPreviewFromForm());
    this.frmGroup.get('monthYear')!.valueChanges.subscribe(() => this.fetchPreviewFromForm());
  }

  loadStudents() {
    this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200 })
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) this.students = rs.data?.data ?? [];
      });
  }

  buildMonthOptions() {
    const now = new Date();
    const months = ['T1','T2','T3','T4','T5','T6','T7','T8','T9','T10','T11','T12'];
    for (let i = 0; i < 12; i++) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const m = d.getMonth() + 1;
      const y = d.getFullYear();
      this.monthOptions.push({
        value: y * 100 + m,
        label: `${months[m - 1]}/${y}${i === 0 ? ' (tháng này)' : i === 1 ? ' (tháng trước)' : ''}`,
        month: m, year: y,
      });
    }
  }

  fetchPreviewFromForm() {
    const v = this.frmGroup.value;
    if (!v.idStudent || !v.monthYear) { this.preview = null; this.currentPeriodId = null; return; }
    const year  = Math.floor(v.monthYear / 100);
    const month = v.monthYear % 100;

    this.isLoadingPreview = true;
    // OpenOrGet để có period id, rồi Preview để có dữ liệu buổi học
    this._service.openOrGet(v.idStudent, month, year).subscribe(open => {
      if (open.status === StatusCode.Ok) {
        this.currentPeriodId = open.data?.id;
        // Nếu period đã closed → không cho close lại ở đây
        if (open.data?.status && open.data.status !== TuitionStatus.Open) {
          this._toast.warning(StatusResponseTitle.WARNING, `Kỳ này đã ở trạng thái "${this.statusLabel(open.data.status)}". Chỉnh sửa ở thẻ kỳ tương ứng.`);
        }
      }
    });
    this._service.preview(v.idStudent, month, year)
      .pipe(finalize(() => this.isLoadingPreview = false))
      .subscribe(rs => {
        if (rs.status === StatusCode.Ok) this.preview = rs.data;
        else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      });
  }

  get totalAfterAdj(): number {
    const total = this.preview?.totalAmount || 0;
    const adj = +(this.frmGroup.value.adjustment || 0);
    return total + adj;
  }

  /** Gom buổi theo môn — hiện breakdown khi HS học ≥2 môn trong kỳ. */
  get subjectGroups(): { subject: string; count: number; amount: number }[] {
    const lessons = this.preview?.lessons ?? [];
    const map = new Map<string, { subject: string; count: number; amount: number }>();
    for (const l of lessons) {
      const key = l.subject || 'Khác';
      const g = map.get(key) ?? { subject: key, count: 0, amount: 0 };
      g.count++; g.amount += l.chargeAmount || 0;
      map.set(key, g);
    }
    return [...map.values()].sort((a, b) => b.amount - a.amount);
  }

  onSave() {
    if (!this.validateForm(this.frmGroup)) {
      this._toast.warning(StatusResponseTitle.WARNING, StatusResponseMessage.INPUT_REQUIRED);
      return;
    }
    if (!this.currentPeriodId) {
      this._toast.error(StatusResponseTitle.ERROR, 'Chưa khởi tạo được kỳ. Thử lại sau.');
      return;
    }
    if (!this.preview || this.preview.totalLessons === 0) {
      this._toast.warning(StatusResponseTitle.WARNING, 'HS chưa có buổi học nào Done trong tháng này');
      return;
    }
    const v = this.frmGroup.value;
    this.isSubmitting = true;
    this._service.close(this.currentPeriodId, +v.adjustment || 0, v.notes || '')
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, 'Đã tính học phí. Hệ thống đã sinh nhắc cho phụ huynh.');
            this.closeModal({ saved: true });
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onCancel() { this.closeModal(); }

  /** Đóng modal + sang Lịch dạy để đánh dấu "Đã dạy" các buổi. */
  goToLessons() {
    this.closeModal();
    this._router.navigate(['/lesson']);
  }

  statusLabel(s: TuitionStatus): string {
    return ({ [TuitionStatus.Open]: 'Đang mở', [TuitionStatus.Closed]: 'Cần thu',
             [TuitionStatus.PartialPaid]: 'Thu 1 phần', [TuitionStatus.Paid]: 'Đã thu' } as any)[s];
  }

  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  fmtDate(s: string): string {
    if (!s) return '';
    const d = new Date(s);
    return `${String(d.getDate()).padStart(2,'0')}/${String(d.getMonth()+1).padStart(2,'0')}`;
  }
}
