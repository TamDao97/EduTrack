import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzAlertModule } from 'ng-zorro-antd/alert';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
  imports: [RouterOutlet, HeaderComponent, SidebarComponent, FooterComponent, NgxSpinnerComponent,NzSpinModule,NzAlertModule],
  standalone: true,
})
export class LayoutComponent implements OnInit {
  constructor() { }

  ngOnInit() { }
}
