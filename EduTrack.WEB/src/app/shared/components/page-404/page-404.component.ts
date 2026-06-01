import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { SharedModule } from '../../modules/shared.module';

@Component({
  selector: 'app-page-404',
  templateUrl: './page-404.component.html',
  styleUrls: ['./page-404.component.scss'],
  standalone: true,
  imports: [SharedModule, RouterModule],
})
export class Page404Component {
  constructor(private _router: Router) {}

  goHome() { this._router.navigate(['/dashboard']); }
  goBack() { history.length > 1 ? history.back() : this.goHome(); }
}
