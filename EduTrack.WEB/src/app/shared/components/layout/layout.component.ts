import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzAlertModule } from 'ng-zorro-antd/alert';

/**
 * Layout chỉ giữ sidebar (rail 96px) + router-outlet + spinner.
 * Đã bỏ kaiadmin top header + footer — avatar/logout/profile chuyển sang trang /settings.
 */
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
  imports: [RouterOutlet, SidebarComponent, NgxSpinnerComponent, NzSpinModule, NzAlertModule],
  standalone: true,
})
export class LayoutComponent implements OnInit {
  constructor() { }
  ngOnInit() { }
}
