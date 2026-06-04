import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { filter } from 'rxjs';
import { ICurrentUser } from '../../../shared/interfaces/ICurrentUser';
import { AuthService } from '../../../shared/utils/services/auth.service';
import { FeedbackService } from '../../../services/tutor-domain/feedback.service';
import { StatusCode } from '../../../shared/utils/enums';

/** 1 mục điều hướng trong Platform Console (hardcode — đây là công cụ nền tảng cố định). */
interface IAdminNav {
  url: string;
  label: string;
  icon: string;
}

/**
 * Shell riêng cho Platform Console (founder, IsSuper). Tách hẳn workspace gia sư:
 * nhãn "Nền tảng EduTrack" + màu tối nghiêm túc + nav cố định 6 mục.
 * Gác bằng SuperGuard ở route cha nên không cần check isSuper ở đây.
 */
@Component({
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, NzIconModule],
})
export class AdminLayoutComponent implements OnInit {
  currentUser?: ICurrentUser;
  sidebarOpen = false;

  readonly navs: IAdminNav[] = [
    { url: '/admin/dashboard', label: 'Tổng quan',   icon: 'dashboard' },
    { url: '/admin/payments',  label: 'Thanh toán',  icon: 'dollar' },
    { url: '/admin/feedback',  label: 'Góp ý',       icon: 'message' },
    { url: '/admin/users',     label: 'Người dùng',  icon: 'team' },
    { url: '/admin/roles',     label: 'Vai trò',     icon: 'safety' },
    { url: '/admin/pages',     label: 'Trang / Menu', icon: 'menu' },
    { url: '/admin/config',    label: 'Cấu hình',    icon: 'setting' },
  ];

  /** Số góp ý "Mới" → badge đỏ trên nav Góp ý. */
  newFeedbackCount = 0;

  constructor(private _router: Router, private _feedbackService: FeedbackService) {}

  ngOnInit(): void {
    const auth = AuthService.getAuthStorage();
    if (auth) this.currentUser = JSON.parse(auth) as ICurrentUser;

    this.loadNewFeedbackCount();

    // Đóng nav khi đổi route (mobile) + refresh badge góp ý
    this._router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        this.sidebarOpen = false;
        this.loadNewFeedbackCount();
      });
  }

  private loadNewFeedbackCount(): void {
    this._feedbackService.newCount().subscribe({
      next: (rs) => { if (rs.status === StatusCode.Ok) this.newFeedbackCount = rs.data ?? 0; },
      error: () => { /* im lặng — badge không quan trọng tới mức báo lỗi */ },
    });
  }

  toggleSidebar(): void { this.sidebarOpen = !this.sidebarOpen; }
  closeSidebar(): void { this.sidebarOpen = false; }

  /** Quay về workspace gia sư (founder vẫn xem được nếu muốn). */
  goToTutorApp(): void { this._router.navigate(['/dashboard']); }

  onLogout(): void {
    AuthService.logout();
    this._router.navigate(['/login']);
  }

  @HostListener('document:keydown.escape')
  onEsc(): void { if (this.sidebarOpen) this.sidebarOpen = false; }
}
