import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { ILessonDetail, LessonStatus, LessonStatusLabel } from '../../../interfaces/ILesson';
import { LessonService } from '../../../services/tutor-domain/lesson.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { LessonBulkFormComponent } from './lesson-bulk-form/lesson-bulk-form.component';
import { LessonFormComponent } from './lesson-form/lesson-form.component';

interface DayGroup {
  date: Date;
  dayLabel: string;     // "Thứ 2"
  dateLabel: string;    // "27/05"
  isToday: boolean;
  lessons: ILessonDetail[];
}

@Component({
  selector: 'app-lesson-week',
  templateUrl: './lesson-week.component.html',
  styleUrls: ['./lesson-week.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class LessonWeekComponent extends TdBaseComponent implements OnInit {
  LessonStatus = LessonStatus;
  LessonStatusLabel = LessonStatusLabel;

  private _service = inject(LessonService);
  private _toast = inject(ToastService);

  /** Thứ 2 của tuần đang xem (00:00) */
  weekStart: Date = this.getMondayOfWeek(new Date());
  weekEnd: Date = this.addDays(this.weekStart, 6);
  days: DayGroup[] = [];
  isLoading = false;

  ngOnInit() { this.load(); }

  load() {
    this.isLoading = true;
    const iso = this.toISODate(this.weekStart);
    this._service.getWeek(iso)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) this.buildDays(rs.data ?? []);
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được lịch tuần'),
      });
  }

  buildDays(lessons: ILessonDetail[]) {
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const groups: DayGroup[] = [];
    for (let i = 0; i < 7; i++) {
      const d = this.addDays(this.weekStart, i);
      const ds = this.toISODate(d);
      groups.push({
        date: d,
        dayLabel: this.dayName(d.getDay()),
        dateLabel: `${this.pad(d.getDate())}/${this.pad(d.getMonth() + 1)}`,
        isToday: ds === this.toISODate(today),
        lessons: lessons
          .filter(l => l.scheduledDate.startsWith(ds))
          .sort((a, b) => a.startTime.localeCompare(b.startTime)),
      });
    }
    this.days = groups;
  }

  onPrevWeek() { this.weekStart = this.addDays(this.weekStart, -7); this.weekEnd = this.addDays(this.weekStart, 6); this.load(); }
  onNextWeek() { this.weekStart = this.addDays(this.weekStart, 7); this.weekEnd = this.addDays(this.weekStart, 6); this.load(); }
  onThisWeek() { this.weekStart = this.getMondayOfWeek(new Date()); this.weekEnd = this.addDays(this.weekStart, 6); this.load(); }

  onAddOne() {
    this.openModal(
      { title: 'Thêm 1 buổi', width: 480, className: 'sheet-bottom-mobile' },
      LessonFormComponent,
      {}
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.load(); });
  }

  onAddRecurring() {
    this.openModal(
      { title: 'Tạo lịch lặp lại', width: 480, className: 'sheet-bottom-mobile' },
      LessonBulkFormComponent,
      {}
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.load(); });
  }

  /** Đánh dấu "Đã dạy" hàng loạt mọi buổi đã qua giờ (toàn bộ, không chỉ tuần đang xem). */
  onMarkDonePast() {
    this.confirmModal(
      'Đánh dấu "Đã dạy" tất cả các buổi đã qua giờ học (chưa thuộc kỳ học phí)?',
      () => {
        this._service.markDonePast().subscribe({
          next: (rs) => {
            if (rs.status === StatusCode.Ok) {
              const n = rs.data ?? 0;
              if (n > 0) this._toast.success(StatusResponseTitle.SUCCESS, `Đã đánh dấu ${n} buổi đã dạy`);
              else this._toast.info(StatusResponseTitle.INFO, 'Không có buổi nào cần đánh dấu');
              this.load();
            } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
          },
          error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
        });
      }
    );
  }

  onMarkDone(lesson: ILessonDetail, ev: Event) {
    ev.stopPropagation();
    if (lesson.idTuitionPeriod) return;
    this._service.markDone(lesson.id!).subscribe({
      next: (rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, 'Đã đánh dấu đã dạy');
          this.load();
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      },
    });
  }

  onCancel(lesson: ILessonDetail, ev: Event) {
    ev.stopPropagation();
    this.confirmModal(`Huỷ buổi ${lesson.studentFullName} ${lesson.startTime}?`, () => {
      this._service.cancel(lesson.id!, 'Huỷ bởi gia sư').subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) { this._toast.success(StatusResponseTitle.SUCCESS, 'Đã huỷ buổi học'); this.load(); }
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
      });
    });
  }

  onEdit(lesson: ILessonDetail) {
    if (lesson.idTuitionPeriod) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Buổi đã chốt vào kỳ học phí — không sửa được');
      return;
    }
    this.openModal(
      { title: 'Sửa buổi học', width: 480, className: 'sheet-bottom-mobile' },
      LessonFormComponent,
      { params: lesson }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.load(); });
  }

  onDelete(lesson: ILessonDetail) {
    if (lesson.idTuitionPeriod) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Buổi đã chốt vào kỳ học phí — không xoá được');
      return;
    }
    this.confirmModal('Xoá buổi học này?', () => {
      this._service.delete(lesson.id!).subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) { this._toast.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.DELETE_SUCCESS); this.load(); }
          else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
      });
    });
  }

  statusColor(s: LessonStatus): string {
    switch (s) {
      case LessonStatus.Scheduled: return 'processing';
      case LessonStatus.Done: return 'success';
      case LessonStatus.Cancelled: return 'default';
    }
  }

  fmtTime(t: string): string {
    if (!t) return '';
    return t.substring(0, 5); // "HH:mm"
  }

  weekRangeLabel(): string {
    return `${this.pad(this.weekStart.getDate())}/${this.pad(this.weekStart.getMonth() + 1)} — ${this.pad(this.weekEnd.getDate())}/${this.pad(this.weekEnd.getMonth() + 1)}/${this.weekEnd.getFullYear()}`;
  }

  trackById(_: number, l: ILessonDetail) { return l.id; }

  /* ─────── date helpers ─────── */
  private getMondayOfWeek(d: Date): Date {
    const x = new Date(d); x.setHours(0, 0, 0, 0);
    const day = x.getDay(); // 0=Sun,1=Mon…6=Sat
    const diff = (day === 0 ? -6 : 1) - day;
    x.setDate(x.getDate() + diff);
    return x;
  }
  private addDays(d: Date, n: number): Date { const x = new Date(d); x.setDate(x.getDate() + n); return x; }
  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${this.pad(d.getMonth() + 1)}-${this.pad(d.getDate())}`;
  }
  private pad(n: number): string { return String(n).padStart(2, '0'); }
  private dayName(d: number): string {
    const map = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
    return map[d];
  }
}
