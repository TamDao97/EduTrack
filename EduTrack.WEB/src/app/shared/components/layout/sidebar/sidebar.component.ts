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

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
  standalone: true,
  imports: [SharedModule],
})
export class SidebarComponent implements OnInit {
  menuItems: IMenu[] = [];

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
    'lesson':      { type: 'calendar',  color: '#0c8599' },
    'inbox':       { type: 'bell',      color: '#faad14' },
    'tuition':     { type: 'dollar',    color: '#c92a2a' },
    'settings':    { type: 'setting',   color: '#475569' },
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
  ) {}

  ngOnInit() {
    this.getPageTreeByUserLogin();
    // Cập nhật active menu khi route đổi
    this._router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => this.updateActiveFromRoute());
    // Lần đầu
    this.updateActiveFromRoute();
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
