import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { LoginService } from '../../../../services/system/login.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { AuthService } from '../../../../shared/utils/services/auth.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [SharedModule, RouterModule],
})
export class LoginComponent extends TdBaseComponent implements OnInit {
  frmGroup!: FormGroup;
  isLoading = false;
  showPassword = false;

  constructor(
    private _router: Router,
    private _fb: FormBuilder,
    private _toastService: ToastService,
    private _loginService: LoginService
  ) {
    super();
  }

  ngOnInit() {
    this.frmGroup = this._fb.group({
      email: ['', [Validators.required]],
      password: [null, [Validators.required, Validators.minLength(6)]],
    });
  }

  togglePassword() { this.showPassword = !this.showPassword; }

  onSubmit() {
    if (!this.validateForm(this.frmGroup)) return;

    this.isLoading = true;
    this._loginService.login(this.frmGroup.value)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (rs) => {
          if (rs.status == StatusCode.Ok) {
            AuthService.setAuthStorage(rs.data);
            this._router.navigate(['/dashboard']);
            this._toastService.success(StatusResponseTitle.SUCCESS, rs.message);
          } else {
            this._toastService.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toastService.error('ERROR', 'Có lỗi xảy ra khi đăng nhập'),
      });
  }
}
