import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { SidebarComponent } from './sidebar/sidebar.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzIconModule } from 'ng-zorro-antd/icon';

/**
 * Layout chỉ giữ sidebar (rail 96px) + router-outlet + spinner.
 * Mobile: sidebar ẩn off-screen, mở qua hamburger; backdrop để đóng.
 */
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
  imports: [CommonModule, RouterOutlet, SidebarComponent, NgxSpinnerComponent, NzSpinModule, NzAlertModule, NzIconModule],
  standalone: true,
})
export class LayoutComponent implements OnInit {
  /** Trạng thái mở của sidebar trên mobile. Desktop luôn hiện, không quan tâm. */
  sidebarOpen = false;

  constructor(private _router: Router) {}

  ngOnInit() {
    // Đóng sidebar khi route đổi (mobile)
    this._router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.sidebarOpen = false);
  }

  toggleSidebar()  { this.sidebarOpen = !this.sidebarOpen; }
  closeSidebar()   { this.sidebarOpen = false; }

  @HostListener('document:keydown.escape')
  onEsc() { if (this.sidebarOpen) this.sidebarOpen = false; }
}
