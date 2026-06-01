import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { ILessonDetail, LessonStatus } from '../../interfaces/ILesson';
import { INotification, NotificationStatus } from '../../interfaces/INotification';
import { StudentStatus } from '../../interfaces/IStudent';
import { ITuitionPeriodDetail, TuitionStatus } from '../../interfaces/ITuitionPeriod';
import { IMySubscription } from '../../interfaces/IBilling';
import { SubscriptionStatus } from '../../interfaces/IAdmin';
import { BillingService } from '../../services/tutor-domain/billing.service';
import { LessonService } from '../../services/tutor-domain/lesson.service';
import { NotificationService } from '../../services/tutor-domain/notification.service';
import { StudentService } from '../../services/tutor-domain/student.service';
import { TuitionPeriodService } from '../../services/tutor-domain/tuition-period.service';
import { SharedModule } from '../../shared/modules/shared.module';
import { AuthService } from '../../shared/utils/services/auth.service';
import { StatusCode } from '../../shared/utils/enums';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class DashboardComponent extends TdBaseComponent implements OnInit {
  private _router = inject(Router);
  private _lessonService = inject(LessonService);
  private _notiService = inject(NotificationService);
  private _studentService = inject(StudentService);
  private _tuitionService = inject(TuitionPeriodService);
  private _billingService = inject(BillingService);

  todayLessons: ILessonDetail[] = [];
  pendingNotifications: INotification[] = [];
  outstandingPeriods: ITuitionPeriodDetail[] = [];
  activeStudentsCount = 0;
  isLoading = false;

  sub: IMySubscription | null = null;
  SubscriptionStatus = SubscriptionStatus;

  tutorName = '';
  todayLabel = this.buildTodayLabel();
  greeting = this.buildGreeting();

  ngOnInit() {
    const authStr = AuthService.getAuthStorage();
    if (authStr) {
      try { this.tutorName = JSON.parse(authStr).displayName || 'bạn'; } catch { this.tutorName = 'bạn'; }
    } else this.tutorName = 'bạn';
    this.loadAll();
  }

  loadAll() {
    this.isLoading = true;
    const today = new Date();
    const weekStart = this.mondayOf(today);

    forkJoin({
      week: this._lessonService.getWeek(this.toISODate(weekStart)),
      inbox: this._notiService.getInbox(),
      students: this._studentService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 500, status: StudentStatus.Active }),
      tuition: this._tuitionService.gridLoadData({ keyword: '', pageNumber: 1, pageSize: 200 }),
      sub: this._billingService.getMine(),
    }).pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: ({ week, inbox, students, tuition, sub }) => {
          if (sub?.status === StatusCode.Ok) this.sub = sub.data;
          if (week?.status === StatusCode.Ok) {
            const todayIso = this.toISODate(today);
            const list: ILessonDetail[] = (week.data || [])
              .filter((l: ILessonDetail) => (l.scheduledDate || '').startsWith(todayIso));
            list.sort((a, b) => (a.startTime || '').localeCompare(b.startTime || ''));
            this.todayLessons = list;
          }
          if (inbox?.status === StatusCode.Ok) {
            this.pendingNotifications = (inbox.data || [])
              .filter((n: INotification) => n.status === NotificationStatus.Pending);
          }
          if (students?.status === StatusCode.Ok) {
            this.activeStudentsCount = students.data?.totalRecord ?? (students.data?.data?.length ?? 0);
          }
          if (tuition?.status === StatusCode.Ok) {
            this.outstandingPeriods = (tuition.data?.data || [])
              .filter((p: ITuitionPeriodDetail) =>
                p.status === TuitionStatus.Closed || p.status === TuitionStatus.PartialPaid);
          }
        },
      });
  }

  get revenueThisMonth(): number {
    const now = new Date();
    return this.outstandingPeriods
      .filter(p => p.periodMonth === now.getMonth() + 1 && p.periodYear === now.getFullYear())
      .reduce((sum, p) => sum + (p.finalAmount || 0), 0);
  }
  get totalOutstanding(): number {
    return this.outstandingPeriods.reduce(
      (sum, p) => sum + (p.outstandingAmount ?? (p.finalAmount - p.paidAmount)), 0
    );
  }
  get lessonsToday(): number { return this.todayLessons.length; }
  get pendingNotiCount(): number { return this.pendingNotifications.length; }
  get pendingTuitionCount(): number { return this.outstandingPeriods.length; }

  goLesson()  { this._router.navigate(['/lesson']); }
  goInbox()   { this._router.navigate(['/inbox']); }
  goTuition() { this._router.navigate(['/tuition']); }
  goStudent() { this._router.navigate(['/student']); }
  goBilling() { this._router.navigate(['/billing']); }

  /** Show banner khi: Expired, hoặc Trial còn ≤3 ngày, hoặc Active còn ≤7 ngày. */
  get showSubBanner(): boolean {
    if (!this.sub) return false;
    if (this.sub.status === SubscriptionStatus.Expired) return true;
    const d = this.sub.daysRemaining ?? 999;
    if (this.sub.status === SubscriptionStatus.Trial && d <= 3) return true;
    if (this.sub.status === SubscriptionStatus.Active && d <= 7) return true;
    return false;
  }
  get subBannerKind(): 'danger' | 'warn' {
    if (!this.sub) return 'warn';
    return this.sub.status === SubscriptionStatus.Expired ? 'danger' : 'warn';
  }
  get subBannerTitle(): string {
    if (!this.sub) return '';
    if (this.sub.status === SubscriptionStatus.Expired) return 'Subscription đã hết hạn';
    const d = this.sub.daysRemaining ?? 0;
    if (this.sub.status === SubscriptionStatus.Trial) {
      return d <= 0 ? 'Trial đã hết' : `Còn ${d} ngày dùng thử`;
    }
    return d <= 0 ? 'Gói đã hết hạn' : `Gói còn ${d} ngày`;
  }
  get subBannerMsg(): string {
    if (!this.sub) return '';
    if (this.sub.status === SubscriptionStatus.Expired)
      return 'Bạn không thể thêm HS / buổi học / kỳ học phí cho đến khi nâng cấp.';
    if (this.sub.status === SubscriptionStatus.Trial)
      return 'Sau khi hết trial bạn sẽ không thể thêm HS / buổi mới — nâng cấp ngay để giữ liền mạch.';
    return 'Gia hạn sớm để không gián đoạn việc dạy.';
  }

  buildGreeting(): string {
    const h = new Date().getHours();
    if (h < 11) return 'Chào buổi sáng,';
    if (h < 14) return 'Chào buổi trưa,';
    if (h < 18) return 'Chào buổi chiều,';
    return 'Chào buổi tối,';
  }
  buildTodayLabel(): string {
    const days = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
    const d = new Date();
    return `${days[d.getDay()]}, ${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  }
  private mondayOf(d: Date): Date {
    const x = new Date(d); x.setHours(0, 0, 0, 0);
    const day = x.getDay();
    x.setDate(x.getDate() + ((day === 0 ? -6 : 1) - day));
    return x;
  }
  private toISODate(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  fmtTime(t?: string): string { return t ? t.substring(0, 5) : ''; }
  fmt(n: number): string { return new Intl.NumberFormat('vi-VN').format(n || 0); }
  initials(name?: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    return (parts[0].charAt(0) + (parts.length > 1 ? parts[parts.length - 1].charAt(0) : '')).toUpperCase();
  }
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
  isLessonDone(l: ILessonDetail): boolean { return l.status === LessonStatus.Done; }
}
