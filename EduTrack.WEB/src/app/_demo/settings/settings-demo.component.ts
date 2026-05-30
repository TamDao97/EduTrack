import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-settings-demo',
  templateUrl: './settings-demo.component.html',
  styleUrls: ['./settings-demo.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, FormsModule],
})
export class SettingsDemoComponent extends TdBaseComponent {
  private _router = inject(Router);

  // Mock state
  tutor = {
    displayName: 'Cô Mai',
    email: 'maitran@example.com',
    phone: '0901234567',
    subjects: 'Toán; Lý',
    bio: 'Gia sư Toán-Lý cho HS THCS & THPT, 5 năm kinh nghiệm.',
  };

  bank = {
    bankName: 'Techcombank',
    accountNumber: '19023456789',
    accountHolder: 'NGUYEN THI MAI',
  };

  app = {
    appName: 'Lớp cô Mai',
    autoNotifyEvening: true,
    autoNotifyHourBefore: true,
    autoCloseTuitionDay1: true,
  };

  subscription = {
    plan: 'Basic',
    expiresAt: '15/07/2026',
    monthsLeft: 1,
  };

  onClose() { this._router.navigate(['/design']); }
  onSave() { /* mock */ alert('(Demo) Đã lưu — chưa gửi API'); }
  onLogout() { /* mock */ alert('(Demo) Đăng xuất'); }
}
