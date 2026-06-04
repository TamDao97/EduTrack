import { Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { PageService } from '../../../../services/system/page.service';
import { ToastService } from '../../../services/toast.service';
import { IResponse } from '../../../interfaces/IResponse';
import { StatusCode } from '../../../utils/enums';
import { StatusResponseTitle } from '../../../utils/constants';
import { SharedModule } from '../../../modules/shared.module';
import { IMenu } from '../../../interfaces/ITree';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../../utils/services/auth.service';
import { ICurrentUser } from '../../../interfaces/ICurrentUser';
import { NotificationService } from '../../../../services/tutor-domain/notification.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
  standalone: true,
  imports: [SharedModule],
})
export class SidebarComponent implements OnInit {
  menuItems: IMenu[] = [];

  /** Founder (IsSuper) → hiện nút quay lại Platform Console ở đáy rail. */
  isSuper = false;

  /** Số nhắc Pending đã đến hạn → badge đỏ trên menu Hộp nhắc. */
  dueCount = 0;
  private _dueCountFetchedAt = 0;

  /** Menu hover hiện tại — null khi không hover gì → ẩn flyout */
  hoveredMenu: IMenu | null = null;

  /** Top offset (px) của flyout — set dựa trên vị trí item hover */
  flyoutTop: number = 0;

  /** Active menu key (parent) — set theo route hiện tại */
  activeMenuKey: any = null;
  /** Active child key */
  activeChildKey: any = null;

  /** Timer delay khi ẩn flyout (để di chuột sang flyout không mất) */
  private hideTimer: any = null;

  /** Map URL → ng-zorro icon type + màu nền. Bổ sung theo nghiệp vụ riêng của dự án. */
  private readonly iconMap: { [key: string]: { type: string; color: string } } = {
    'dashboard':   { type: 'dashboard', color: '#3b5bdb' },
    'student':     { type: 'solution',  color: '#f76707' },
    'classroom':   { type: 'cluster',   color: '#7C3AED' },
    'lesson':      { type: 'calendar',  color: '#0c8599' },
    'inbox':       { type: 'bell',      color: '#faad14' },
    'tuition':     { type: 'dollar',    color: '#c92a2a' },
    'report':      { type: 'bar-chart', color: '#0F766E' },
    'billing':     { type: 'credit-card', color: '#7C3AED' },
    'settings':    { type: 'setting',   color: '#475569' },
    'feedback':    { type: 'message',   color: '#DB2777' },
    'admin':       { type: 'crown',     color: '#1A1F36' },
    'user':        { type: 'team',      color: '#1d4ed8' },
    'role':        { type: 'safety',    color: '#65a30d' },
    'page':        { type: 'menu',      color: '#64748b' },
    'config-json': { type: 'setting',   color: '#475569' },
  };

  /** Default icon khi không match */
  private readonly defaultIcon = { type: 'folder', color: '#64748b' };

  /** Lookup icon theo URL (fallback) */
  private getIconByUrl(url: string | undefined): { type: string; color: string } {
    if (!url) return this.defaultIcon;
    const key = url.toLowerCase().replace(/^\/+/, '').split('/')[0];
    return this.iconMap[key] ?? this.defaultIcon;
  }

  /**
   * Trả icon cho 1 menu — ưu tiên `menu.icon` user cấu hình ở DB.
   * Format hỗ trợ:
   *   "shopping-cart"            → icon ng-zorro, color theo URL map
   *   "shopping-cart|#ff5722"    → icon + color custom (hex)
   *   ""/null                    → fallback theo URL map
   */
  getMenuIcon(menu?: { icon?: string; url?: string }): { type: string; color: string } {
    if (!menu) return this.defaultIcon;
    const raw = (menu.icon || '').trim();
    if (raw) {
      const [typeRaw, colorRaw] = raw.split('|');
      const type = typeRaw?.trim();
      const customColor = colorRaw?.trim();
      if (type) {
        const fallbackColor = this.getIconByUrl(menu.url).color;
        return { type, color: customColor || fallbackColor };
      }
    }
    return this.getIconByUrl(menu.url);
  }

  /** Trả icon cho parent — ưu tiên icon config, fallback URL, fallback child, fallback title keyword */
  getParentIcon(menu: IMenu): { type: string; color: string } {
    // 1. User cấu hình icon trên chính parent
    if ((menu.icon || '').trim()) return this.getMenuIcon(menu);
    // 2. Parent có URL riêng → dùng URL map
    if (menu.url) return this.getMenuIcon(menu);
    // 3. Group menu không URL → mượn icon child đầu
    if (menu.children && menu.children.length) {
      const firstChild = menu.children[0];
      if (firstChild.icon || firstChild.url) return this.getMenuIcon(firstChild);
    }
    // 4. Suy ra từ keyword trong title
    const t = (menu.title || '').toLowerCase();
    if (t.includes('dashboard') || t.includes('tổng quan')) return this.iconMap['dashboard'];
    if (t.includes('người dùng') || t.includes('user')) return this.iconMap['user'];
    if (t.includes('vai trò') || t.includes('quyền') || t.includes('role')) return this.iconMap['role'];
    if (t.includes('menu') || t.includes('trang')) return this.iconMap['page'];
    if (t.includes('cấu hình') || t.includes('config') || t.includes('setting')) return this.iconMap['config-json'];
    return this.defaultIcon;
  }

  constructor(
    private _fb: FormBuilder,
    private _toastService: ToastService,
    private _pageService: PageService,
    private _router: Router,
    private _notificationService: NotificationService,
  ) {}

  ngOnInit() {
    const authStr = AuthService.getAuthStorage();
    if (authStr) {
      try { this.isSuper = !!(JSON.parse(authStr) as ICurrentUser).isSuper; } catch { this.isSuper = false; }
    }
    this.getPageTreeByUserLogin();
    // Cập nhật active menu khi route đổi
    this._router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveFromRoute();
        this.fetchDueCount(); // refresh badge khi điều hướng (throttle 60s)
      });
    // Lần đầu
    this.updateActiveFromRoute();
    this.fetchDueCount();
  }

  /** Lấy số nhắc đến hạn cho badge — throttle 60s để không spam API. */
  private fetchDueCount(): void {
    const now = Date.now();
    if (now - this._dueCountFetchedAt < 60_000) return;
    this._dueCountFetchedAt = now;
    this._notificationService.getDueCount().subscribe({
      next: (rs) => { if (rs.status === StatusCode.Ok) this.dueCount = rs.data ?? 0; },
      error: () => { /* không có quyền / lỗi mạng → giữ badge cũ, không toast */ },
    });
  }

  /** Hiển thị badge: 1-9 giữ nguyên, >9 → "9+". */
  get dueBadge(): string {
    return this.dueCount > 9 ? '9+' : String(this.dueCount);
  }

  getPageTreeByUserLogin() {
    this._pageService.getPageTreeByUserLogin().subscribe((rs: IResponse) => {
      if (rs.status == StatusCode.Ok) {
        this.menuItems = rs.data;
        this.updateActiveFromRoute();
      } else {
        this._toastService.error(StatusResponseTitle.ERROR, rs.message);
      }
    });
  }

  /** Tính active dựa trên URL hiện tại (so với menu.url) */
  private updateActiveFromRoute(): void {
    const url = this._router.url.replace(/^\//, '').split('?')[0];
    this.activeMenuKey = null;
    this.activeChildKey = null;
    for (const m of this.menuItems || []) {
      if (m.url && url.startsWith(m.url)) {
        this.activeMenuKey = m.key;
        return;
      }
      for (const c of m.children || []) {
        if (c.url && url.startsWith(c.url)) {
          this.activeMenuKey = m.key;
          this.activeChildKey = c.key;
          return;
        }
      }
    }
  }

  /** Hover vào icon parent → show flyout (set top theo offset của item) */
  onMenuEnter(menu: IMenu, ev: MouseEvent): void {
    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
      this.hideTimer = null;
    }
    this.hoveredMenu = menu;
    const target = ev.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    this.flyoutTop = rect.top;
  }

  /** Rời chuột → delay 150ms để di sang flyout */
  onMenuLeave(): void {
    this.hideTimer = setTimeout(() => {
      this.hoveredMenu = null;
    }, 150);
  }

  /** Hover vào flyout → giữ */
  onFlyoutEnter(): void {
    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
      this.hideTimer = null;
    }
  }
  onFlyoutLeave(): void {
    this.hoveredMenu = null;
  }

  /** Founder quay lại Platform Console từ workspace gia sư. */
  goToAdmin(): void {
    this.hoveredMenu = null;
    this._router.navigate(['/admin']);
  }

  /** Click 1 item → ẩn flyout + navigate (nếu có url) */
  onItemClick(menu: IMenu): void {
    this.hoveredMenu = null;
    if (menu.url) {
      this._router.navigate(['/' + menu.url]);
    }
  }

  /** Đóng flyout khi click ra ngoài */
  @HostListener('document:click', ['$event'])
  onDocClick(ev: MouseEvent): void {
    const target = ev.target as HTMLElement;
    if (!target.closest('.sidebar-mini') && !target.closest('.sidebar-flyout')) {
      this.hoveredMenu = null;
    }
  }
}
