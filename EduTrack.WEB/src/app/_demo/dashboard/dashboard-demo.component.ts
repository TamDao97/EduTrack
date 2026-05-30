import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

interface LessonToday {
  time: string;
  studentName: string;
  subject: string;
  location: string;
  status: 'upcoming' | 'done' | 'cancelled';
  avatarFrom: string;
  avatarTo: string;
}

interface PendingTask {
  type: 'reminder' | 'tuition';
  title: string;
  sub?: string;
  amount?: number;
  daysOverdue?: number;
}

@Component({
  selector: 'app-dashboard-demo',
  templateUrl: './dashboard-demo.component.html',
  styleUrls: ['./dashboard-demo.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class DashboardDemoComponent extends TdBaseComponent {
  private _router = inject(Router);

  todayLabel = 'Thứ 7, 30/05/2026';
  greeting = 'Chào buổi sáng,';
  tutorName = 'Cô Mai';

  todayLessons: LessonToday[] = [
    { time: '14:00', studentName: 'Nguyễn Minh Mai', subject: 'Toán · Lớp 12', location: 'Tại nhà HS', status: 'upcoming', avatarFrom: '#FFA1BD', avatarTo: '#FF6B9D' },
    { time: '17:30', studentName: 'Trần Đức Nam', subject: 'Lý · Lớp 11', location: 'Online Zoom', status: 'upcoming', avatarFrom: '#7C7FE0', avatarTo: '#5B5FCF' },
    { time: '19:30', studentName: 'Lê Phương Hoa', subject: 'Anh · Lớp 10', location: 'TT Cầu Giấy', status: 'done', avatarFrom: '#4ECDC4', avatarTo: '#00B8A9' },
  ];

  pendingTasks: PendingTask[] = [
    { type: 'reminder', title: '5 nhắc cần gửi', sub: 'Gửi nhắc buổi học mai cho phụ huynh' },
    { type: 'tuition', title: 'HP Mai · tháng 5', amount: 1500000, daysOverdue: 3 },
    { type: 'tuition', title: 'HP Nam · tháng 5', amount: 1200000, daysOverdue: 7 },
  ];

  monthStats = {
    revenue: 12300000,
    lessonsDone: 48,
    activeStudents: 8,
    pendingPeriods: 3,
  };

  initials(name: string): string {
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  onClose() { this._router.navigate(['/design']); }
}
