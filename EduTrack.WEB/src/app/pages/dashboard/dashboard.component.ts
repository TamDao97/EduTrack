import { Component, OnInit } from '@angular/core';
import { SharedModule } from '../../shared/modules/shared.module';
import { TdBaseComponent } from '../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [SharedModule],
})
export class DashboardComponent extends TdBaseComponent implements OnInit {
  constructor() {
    super();
  }

  ngOnInit() {}
}
