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
import { LessonGroupFormComponent } from './lesson-group-form/lesson-group-form.component';
import { SessionEditFormComponent } from './session-edit-form/session-edit-form.component';
import { LessonFormComponent } from './lesson-form/lesson-form.component';

/** 1 CA nhóm trong ngày — các lesson cùng groupKey gộp thành 1 thẻ "chiều lớp học". */
interface SessionGroup {
  groupKey: string;
  className?: string | null;
  courseSubject?: string | null;
  startTime: string;
  endTime: string;
  location?: string | null;
  lessons: ILessonDetail[];
  total: number;
  scheduledCount: number;
  doneCount: number;
}

/** 1 mục hiển thị trong ngày: buổi 1-1 lẻ hoặc 1 ca nhóm. */
type DayItem =
  | { kind: 'single'; startTime: string; endTime: string; lesson: ILessonDetail; conflict?: boolean }
  | { kind: 'group'; startTime: string; endTime: string; session: SessionGroup; conflict?: boolean };

interface DayGroup {
  date: Date;
  iso: string;          // "2026-06-05" — key cho thu gọn ngày quá khứ
  dayLabel: string;     // "Thứ 2"
  dateLabel: string;    // "27/05"
  isToday: boolean;
  isPast: boolean;
  lessons: ILessonDetail[];
  items: DayItem[];
}

/** Tổng quan tuần/tháng — dải số đầu màn. */
interface WeekSummary {
  totalLessons: number;   // buổi-HS (không tính đã huỷ)
  doneLessons: number;
  todayLessons: number;   // buổi hôm nay chưa huỷ
  teachHours: number;     // giờ dạy thật (mỗi CA tính 1 lần, không nhân theo số HS)
}

/** 1 ô ngày trên lưới tháng — dot mật độ theo CA (nhóm tính 1, buổi 1-1 tính 1). */
interface MonthCell {
  date: Date;
  iso: string;
  dayNum: number;
  inMonth: boolean;       // ô thuộc tháng đang xem (ô lấp tuần đầu/cuối thì mờ + disabled)
  isToday: boolean;
  total: number;          // buổi-HS chưa huỷ
  doneCount: number;      // buổi-HS đã dạy
  dots: ('scheduled' | 'done')[];  // tối đa MAX_CELL_DOTS dot
  extra: number;          // số CA vượt quá số dot hiển thị
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

  /* ─── Chế độ xem: Tuần (mặc định, tác nghiệp hàng ngày) / Tháng (toàn cảnh) ─── */
  viewMode: 'week' | 'month' = 'week';
  /** Ngày 1 của tháng đang xem (mode tháng) */
  monthStart: Date = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  monthWeeks: MonthCell[][] = [];
  /** Ngày đang chọn trên lưới tháng — xổ chi tiết buổi bên dưới */
  selectedDayIso: string | null = null;
  selectedDayGroup: DayGroup | null = null;
  readonly dowLabels = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

  /** Toàn bộ buổi của tuần (chưa lọc) — bộ lọc áp client-side trên đây. */
  private rawLessons: ILessonDetail[] = [];

  /* ─── Bộ lọc trong tuần (client-side — data tuần đã tải sẵn) ─── */
  filterClass: string | null = null;     // tên lớp, hoặc '1-1' = buổi kèm riêng
  filterStudent: string | null = null;   // tên HS
  filterStatus: LessonStatus | null = null;
  /** Options dựng từ data tuần hiện tại */
  classOptions: string[] = [];
  studentOptions: string[] = [];
  statusOptions = [
    { value: LessonStatus.Scheduled, label: 'Sắp tới' },
    { value: LessonStatus.Done,      label: 'Đã dạy' },
    { value: LessonStatus.Cancelled, label: 'Đã huỷ' },
  ];

  get hasFilter(): boolean {
    return this.filterClass !== null || this.filterStudent !== null || this.filterStatus !== null;
  }

  onFilterChange() { this.rebuild(); }

  onClearFilters() {
    this.filterClass = this.filterStudent = null;
    this.filterStatus = null;
    this.rebuild();
  }

  private applyFilters(lessons: ILessonDetail[]): ILessonDetail[] {
    return lessons.filter(l => {
      if (this.filterClass === '1-1' && l.groupKey) return false;
      if (this.filterClass && this.filterClass !== '1-1' && l.className !== this.filterClass) return false;
      if (this.filterStudent && l.studentFullName !== this.filterStudent) return false;
      if (this.filterStatus !== null && l.status !== this.filterStatus) return false;
      return true;
    });
  }

  ngOnInit() { this.load(); }

  load() {
    this.isLoading = true;
    const req = this.viewMode === 'week'
      ? this._service.getWeek(this.toISODate(this.weekStart))
      : this._service.getRange(this.toISODate(this.monthStart), this.toISODate(this.monthEnd()));
    req.pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this.rawLessons = rs.data ?? [];
            // Dựng options từ khoảng thời gian hiện tại
            this.classOptions = [...new Set(this.rawLessons.map(l => l.className).filter(Boolean))] as string[];
            this.studentOptions = [...new Set(this.rawLessons.map(l => l.studentFullName).filter(Boolean))].sort() as string[];
            this.rebuild();
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Không tải được lịch dạy'),
      });
  }

  /** Dựng lại view từ rawLessons theo mode hiện tại (filter áp client-side). */
  private rebuild() {
    if (this.viewMode === 'week') this.buildDays(this.rawLessons);
    else this.buildMonth(this.rawLessons);
  }

  /** Tổng quan tuần (tính từ rawLessons, KHÔNG phụ thuộc bộ lọc). */
  summary: WeekSummary = { totalLessons: 0, doneLessons: 0, todayLessons: 0, teachHours: 0 };

  buildDays(lessons: ILessonDetail[]) {
    const filtered = this.applyFilters(lessons);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const todayIso = this.toISODate(today);
    const groups: DayGroup[] = [];
    for (let i = 0; i < 7; i++) {
      const d = this.addDays(this.weekStart, i);
      const ds = this.toISODate(d);
      const dayLessons = filtered
        .filter(l => l.scheduledDate.startsWith(ds))
        .sort((a, b) => a.startTime.localeCompare(b.startTime));
      groups.push({
        date: d,
        iso: ds,
        dayLabel: this.dayName(d.getDay()),
        dateLabel: `${this.pad(d.getDate())}/${this.pad(d.getMonth() + 1)}`,
        isToday: ds === todayIso,
        isPast: ds < todayIso,
        lessons: dayLessons,
        items: this.buildDayItems(dayLessons),
      });
    }
    this.days = groups;
    this.summary = this.buildSummary(this.rawLessons, todayIso);
  }

  /** Dải tổng quan tuần: buổi / đã dạy / hôm nay / giờ dạy (mỗi CA tính 1 lần). */
  private buildSummary(lessons: ILessonDetail[], todayIso: string): WeekSummary {
    const active = lessons.filter(l => l.status !== LessonStatus.Cancelled);
    const minutes = (s: string, e: string) => {
      const [sh, sm] = s.split(':').map(Number); const [eh, em] = e.split(':').map(Number);
      return Math.max(0, (eh * 60 + em) - (sh * 60 + sm));
    };
    // Giờ dạy thật: 1 CA (groupKey) = 1 lần; buổi 1-1 theo từng buổi
    const seen = new Set<string>();
    let mins = 0;
    for (const l of active) {
      const key = l.groupKey ? `g:${l.groupKey}` : `s:${l.id}`;
      if (seen.has(key)) continue;
      seen.add(key);
      mins += minutes(l.startTime, l.endTime);
    }
    return {
      totalLessons: active.length,
      doneLessons: active.filter(l => l.status === LessonStatus.Done).length,
      todayLessons: active.filter(l => l.scheduledDate.startsWith(todayIso)).length,
      teachHours: Math.round((mins / 60) * 10) / 10,
    };
  }

  /* ─── VIEW THÁNG: lưới mật độ + chi tiết ngày được chọn ─── */

  /** Tháng trống hoàn toàn → empty state dẫn về nút "Thêm lịch". */
  get hasNoLessons(): boolean { return this.rawLessons.length === 0; }

  private static readonly MAX_CELL_DOTS = 4;

  buildMonth(lessons: ILessonDetail[]) {
    const filtered = this.applyFilters(lessons);
    const todayIso = this.toISODate(new Date());
    // Gom buổi theo ngày — tra nhanh khi dựng từng ô
    const byDate = new Map<string, ILessonDetail[]>();
    for (const l of filtered) {
      const iso = l.scheduledDate.substring(0, 10);
      const arr = byDate.get(iso);
      if (arr) arr.push(l); else byDate.set(iso, [l]);
    }
    const weeks: MonthCell[][] = [];
    const monthEnd = this.monthEnd();
    let cursor = this.getMondayOfWeek(this.monthStart);
    while (cursor <= monthEnd) {
      weeks.push(Array.from({ length: 7 }, (_, i) =>
        this.buildMonthCell(this.addDays(cursor, i), todayIso, byDate)));
      cursor = this.addDays(cursor, 7);
    }
    this.monthWeeks = weeks;
    this.summary = this.buildSummary(this.rawLessons, todayIso);
    this.refreshSelectedDay(byDate, todayIso);
  }

  private buildMonthCell(d: Date, todayIso: string, byDate: Map<string, ILessonDetail[]>): MonthCell {
    const iso = this.toISODate(d);
    const dayLessons = (byDate.get(iso) ?? [])
      .slice().sort((a, b) => a.startTime.localeCompare(b.startTime));
    const active = dayLessons.filter(l => l.status !== LessonStatus.Cancelled);
    const dots = this.buildCellDots(dayLessons);
    const max = LessonWeekComponent.MAX_CELL_DOTS;
    return {
      date: d, iso, dayNum: d.getDate(),
      inMonth: d.getMonth() === this.monthStart.getMonth(),
      isToday: iso === todayIso,
      total: active.length,
      doneCount: active.filter(l => l.status === LessonStatus.Done).length,
      dots: dots.slice(0, max),
      extra: Math.max(0, dots.length - max),
    };
  }

  /** Dot mật độ 1 ngày: mỗi CA 1 dot — xanh lá = đã dạy trọn ca, xanh tím = còn buổi sắp tới. Ca huỷ toàn bộ không vẽ. */
  private buildCellDots(dayLessons: ILessonDetail[]): ('scheduled' | 'done')[] {
    const dots: ('scheduled' | 'done')[] = [];
    for (const it of this.buildDayItems(dayLessons)) {
      if (it.kind === 'single') {
        if (it.lesson.status === LessonStatus.Cancelled) continue;
        dots.push(it.lesson.status === LessonStatus.Done ? 'done' : 'scheduled');
      } else {
        if (it.session.scheduledCount === 0 && it.session.doneCount === 0) continue;
        dots.push(it.session.scheduledCount === 0 ? 'done' : 'scheduled');
      }
    }
    return dots;
  }

  /** Dựng lại DayGroup của ngày đang chọn — null nếu chưa chọn ngày nào. */
  private refreshSelectedDay(byDate: Map<string, ILessonDetail[]>, todayIso: string) {
    if (!this.selectedDayIso) { this.selectedDayGroup = null; return; }
    const d = this.parseISODate(this.selectedDayIso);
    const dayLessons = (byDate.get(this.selectedDayIso) ?? [])
      .slice().sort((a, b) => a.startTime.localeCompare(b.startTime));
    this.selectedDayGroup = {
      date: d,
      iso: this.selectedDayIso,
      dayLabel: this.dayName(d.getDay()),
      dateLabel: `${this.pad(d.getDate())}/${this.pad(d.getMonth() + 1)}`,
      isToday: this.selectedDayIso === todayIso,
      isPast: this.selectedDayIso < todayIso,
      lessons: dayLessons,
      items: this.buildDayItems(dayLessons),
    };
  }

  onSelectDay(cell: MonthCell) {
    if (!cell.inMonth) return;
    this.selectedDayIso = cell.iso;
    this.buildMonth(this.rawLessons); // data đã ở client — dựng lại rẻ
  }

  /* ─── Chuyển chế độ xem Tuần ⇄ Tháng ─── */
  setViewMode(mode: 'week' | 'month') {
    if (this.viewMode === mode) return;
    this.viewMode = mode;
    if (mode === 'month') {
      this.monthStart = new Date(this.weekStart.getFullYear(), this.weekStart.getMonth(), 1);
      this.selectedDayIso = this.isoInCurrentMonth(this.toISODate(new Date()));
    } else {
      // Về tuần chứa ngày đang chọn — giữ mạch ngữ cảnh đang xem
      const base = this.selectedDayIso ? this.parseISODate(this.selectedDayIso) : this.monthStart;
      this.weekStart = this.getMondayOfWeek(base);
      this.weekEnd = this.addDays(this.weekStart, 6);
    }
    this.load();
  }

  /** iso nếu thuộc tháng đang xem, ngược lại null (tháng khác thì chưa chọn ngày nào). */
  private isoInCurrentMonth(iso: string): string | null {
    const monthKey = `${this.monthStart.getFullYear()}-${this.pad(this.monthStart.getMonth() + 1)}`;
    return iso.startsWith(monthKey) ? iso : null;
  }

  /** Ngày cuối của tháng đang xem. */
  private monthEnd(): Date {
    return new Date(this.monthStart.getFullYear(), this.monthStart.getMonth() + 1, 0);
  }

  /* ─── Thu gọn ngày đã qua — hôm nay luôn mở, quá khứ bấm mới xổ ─── */
  expandedPastDays = new Set<string>();
  isDayOpen(day: DayGroup): boolean { return !day.isPast || this.expandedPastDays.has(day.iso); }
  toggleDayOpen(day: DayGroup) {
    if (!day.isPast) return;
    if (this.expandedPastDays.has(day.iso)) this.expandedPastDays.delete(day.iso);
    else this.expandedPastDays.add(day.iso);
  }
  doneCountOf(day: DayGroup): number {
    return day.lessons.filter(l => l.status === LessonStatus.Done).length;
  }

  /** Nhảy nhanh tới tuần/tháng chứa ngày bất kỳ (date-picker trên nav). */
  onJumpToDate(d: Date | null) {
    if (!d) return;
    if (this.viewMode === 'week') {
      this.weekStart = this.getMondayOfWeek(d);
      this.weekEnd = this.addDays(this.weekStart, 6);
    } else {
      this.monthStart = new Date(d.getFullYear(), d.getMonth(), 1);
      this.selectedDayIso = this.toISODate(d); // mode tháng: nhảy là chọn luôn ngày đó
    }
    this.load();
  }

  /** View "chiều lớp học": gộp các lesson cùng groupKey thành 1 thẻ CA; buổi 1-1 giữ nguyên. */
  private buildDayItems(dayLessons: ILessonDetail[]): DayItem[] {
    const items: DayItem[] = [];
    const sessions = new Map<string, SessionGroup>();
    for (const l of dayLessons) {
      if (!l.groupKey) {
        items.push({ kind: 'single', startTime: l.startTime, endTime: l.endTime, lesson: l });
        continue;
      }
      let s = sessions.get(l.groupKey);
      if (!s) {
        s = {
          groupKey: l.groupKey,
          className: l.className,
          courseSubject: l.courseSubject,
          startTime: l.startTime,
          endTime: l.endTime,
          location: l.location,
          lessons: [],
          total: 0, scheduledCount: 0, doneCount: 0,
        };
        sessions.set(l.groupKey, s);
        items.push({ kind: 'group', startTime: l.startTime, endTime: l.endTime, session: s });
      }
      s.lessons.push(l);
      s.total += l.chargeAmount || 0;
      if (l.status === LessonStatus.Scheduled) s.scheduledCount++;
      if (l.status === LessonStatus.Done) s.doneCount++;
    }
    items.sort((a, b) => a.startTime.localeCompare(b.startTime));
    this.markConflicts(items);
    return items;
  }

  /** ⚠️ Cảnh báo TRÙNG GIỜ: 2 mục cùng ngày giao nhau về khung giờ (bỏ qua đã huỷ). */
  private markConflicts(items: DayItem[]) {
    const isActive = (it: DayItem) =>
      it.kind === 'single'
        ? it.lesson.status !== LessonStatus.Cancelled
        : (it.session.scheduledCount + it.session.doneCount) > 0;
    const act = items.filter(isActive);
    for (let i = 0; i < act.length; i++) {
      for (let j = i + 1; j < act.length; j++) {
        // [start, end) giao nhau — so chuỗi "HH:mm:ss" hợp lệ
        if (act[i].startTime < act[j].endTime && act[j].startTime < act[i].endTime) {
          act[i].conflict = true;
          act[j].conflict = true;
        }
      }
    }
  }

  /* ─── Thao tác theo CA (nhóm) ─── */

  /** Ca nào đang mở xổ danh sách HS — giữ qua reload */
  expandedSessions = new Set<string>();

  toggleSession(s: SessionGroup) {
    if (this.expandedSessions.has(s.groupKey)) this.expandedSessions.delete(s.groupKey);
    else this.expandedSessions.add(s.groupKey);
  }
  isExpanded(s: SessionGroup): boolean { return this.expandedSessions.has(s.groupKey); }

  /** Sửa CẢ CA: đổi ngày/giờ/địa điểm đồng loạt (per-em chỉ sửa được ghi chú). */
  onEditSession(s: SessionGroup, ev: Event) {
    ev.stopPropagation();
    const first = s.lessons[0];
    this.openModal(
      { title: `Sửa cả ca — ${s.className || 'Nhóm'}`, width: 460, className: 'sheet-bottom-mobile' },
      SessionEditFormComponent,
      { params: {
          groupKey: s.groupKey,
          className: s.className,
          scheduledDate: first?.scheduledDate,
          startTime: s.startTime,
          endTime: s.endTime,
          location: s.location,
          scheduledCount: s.scheduledCount,
        } }
    ).afterClose.subscribe((rs) => { if (rs?.saved) this.load(); });
  }

  onMarkDoneSession(s: SessionGroup, ev: Event) {
    ev.stopPropagation();
    this._service.markDoneGroup(s.groupKey).subscribe({
      next: (rs) => {
        if (rs.status === StatusCode.Ok) {
          this._toast.success(StatusResponseTitle.SUCCESS, `Đã đánh dấu cả ca (${rs.data} buổi)`);
          this.load();
        } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
      },
      error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
    });
  }

  onCancelSession(s: SessionGroup, ev: Event) {
    ev.stopPropagation();
    this.confirmModal(`Huỷ CẢ CA ${s.className || 'nhóm'} (${s.scheduledCount} buổi chưa dạy)?`, () => {
      this._service.cancelGroup(s.groupKey, 'Huỷ cả ca bởi gia sư').subscribe({
        next: (rs) => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, `Đã huỷ ${rs.data} buổi của ca`);
            this.load();
          } else this._toast.error(StatusResponseTitle.ERROR, rs.message);
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
    });
  }

  fmtVnd(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }

  onPrevPeriod() { if (this.viewMode === 'week') this.shiftWeek(-7); else this.shiftMonth(-1); }
  onNextPeriod() { if (this.viewMode === 'week') this.shiftWeek(7); else this.shiftMonth(1); }

  onThisPeriod() {
    const now = new Date();
    if (this.viewMode === 'week') {
      this.weekStart = this.getMondayOfWeek(now);
      this.weekEnd = this.addDays(this.weekStart, 6);
    } else {
      this.monthStart = new Date(now.getFullYear(), now.getMonth(), 1);
      this.selectedDayIso = this.toISODate(now);
    }
    this.load();
  }

  private shiftWeek(days: number) {
    this.weekStart = this.addDays(this.weekStart, days);
    this.weekEnd = this.addDays(this.weekStart, 6);
    this.load();
  }

  private shiftMonth(months: number) {
    this.monthStart = new Date(this.monthStart.getFullYear(), this.monthStart.getMonth() + months, 1);
    this.selectedDayIso = this.isoInCurrentMonth(this.toISODate(new Date()));
    this.load();
  }

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

  onAddGroup() {
    this.openModal(
      { title: 'Thêm buổi học nhóm', width: 560, className: 'sheet-bottom-mobile' },
      LessonGroupFormComponent,
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
      this._toast.warning(StatusResponseTitle.WARNING, 'Buổi đã vào hoá đơn học phí — không sửa được');
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
      this._toast.warning(StatusResponseTitle.WARNING, 'Buổi đã vào hoá đơn học phí — không xoá được');
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

  navLabel(): string {
    if (this.viewMode === 'month') return `Tháng ${this.monthStart.getMonth() + 1}/${this.monthStart.getFullYear()}`;
    return `${this.pad(this.weekStart.getDate())}/${this.pad(this.weekStart.getMonth() + 1)} — ${this.pad(this.weekEnd.getDate())}/${this.pad(this.weekEnd.getMonth() + 1)}/${this.weekEnd.getFullYear()}`;
  }

  navHint(): string {
    return this.viewMode === 'week' ? 'Bấm để về tuần này' : 'Bấm để về tháng này';
  }

  /** Mốc đang xem cho date-picker nhảy nhanh. */
  get navAnchor(): Date { return this.viewMode === 'week' ? this.weekStart : this.monthStart; }

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
  /** Parse "yyyy-MM-dd" theo giờ ĐỊA PHƯƠNG — new Date(iso) parse UTC, lệch ngày ở timezone âm. */
  private parseISODate(iso: string): Date {
    const [y, m, d] = iso.split('-').map(Number);
    return new Date(y, m - 1, d);
  }
  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${this.pad(d.getMonth() + 1)}-${this.pad(d.getDate())}`;
  }
  private pad(n: number): string { return String(n).padStart(2, '0'); }
  private dayName(d: number): string {
    const map = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
    return map[d];
  }
}
