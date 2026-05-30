import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SharedModule } from '../shared/modules/shared.module';

interface DemoItem {
  key: string;
  title: string;
  desc: string;
  status: 'ready' | 'wip' | 'planned';
  route?: string;
  iconType: string;
  gradient: string;
}

@Component({
  selector: 'app-design-gallery',
  templateUrl: './design-gallery.component.html',
  styleUrls: ['./design-gallery.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class DesignGalleryComponent {
  private _router = inject(Router);

  items: DemoItem[] = [
    {
      key: 'onboarding', title: 'Onboarding 3-step', status: 'ready',
      desc: 'Welcome → profile → student → lesson → done',
      route: '/design/onboarding',
      iconType: 'rocket',
      gradient: 'linear-gradient(135deg, #5B5FCF 0%, #7C3AED 100%)',
    },
    {
      key: 'dashboard', title: 'Dashboard', status: 'ready',
      desc: 'Trang chủ — chào theo giờ, lịch hôm nay, cần xử lý, stats T5, lối tắt',
      route: '/design/dashboard',
      iconType: 'dashboard',
      gradient: 'linear-gradient(135deg, #FF8A65 0%, #FF6B9D 100%)',
    },
    {
      key: 'tuition', title: 'Kỳ học phí', status: 'ready',
      desc: 'List kỳ tháng với progress bar, status tabs, hành động chốt/thu/nhắc',
      route: '/design/tuition',
      iconType: 'dollar',
      gradient: 'linear-gradient(135deg, #00B8A9 0%, #52C41A 100%)',
    },
    {
      key: 'student-table', title: 'HS — TABLE mode', status: 'ready',
      desc: 'Bảng truyền thống có filter + bulk action, để so sánh với card view',
      route: '/design/student-table',
      iconType: 'table',
      gradient: 'linear-gradient(135deg, #4A5170 0%, #1A1F36 100%)',
    },
    {
      key: 'settings', title: 'Cài đặt + Hồ sơ', status: 'ready',
      desc: 'Profile, bank info VietQR, app preferences với toggle switch',
      route: '/design/settings',
      iconType: 'setting',
      gradient: 'linear-gradient(135deg, #7C3AED 0%, #EC4899 100%)',
    },
    {
      key: 'tuition-close', title: 'Chốt kỳ + Thanh toán', status: 'planned',
      desc: 'Modal chốt kỳ với VietQR + tracking thanh toán',
      iconType: 'check-circle',
      gradient: 'linear-gradient(135deg, #FAAD14 0%, #F97316 100%)',
    },
    {
      key: 'landing', title: 'Landing page', status: 'planned',
      desc: 'Trang bán public — hero + features + pricing + CTA',
      iconType: 'global',
      gradient: 'linear-gradient(135deg, #7C3AED 0%, #EC4899 100%)',
    },
  ];

  statusLabel(s: DemoItem['status']): string {
    return { ready: 'Sẵn sàng', wip: 'Đang làm', planned: 'Chưa làm' }[s];
  }

  open(item: DemoItem) {
    if (item.route) this._router.navigate([item.route]);
  }
}
