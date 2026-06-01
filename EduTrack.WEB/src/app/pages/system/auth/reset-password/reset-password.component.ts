import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { LoginService } from '../../../../services/system/login.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['../forgot-password/forgot-password.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, RouterModule],
})
export class ResetPasswordComponent extends TdBaseComponent implements OnInit {
  frmGroup!: FormGroup;
  isLoading = false;
  showPassword = false;
  token = '';
  done = false;

  constructor(
    private _fb: FormBuilder,
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: LoginService,
    private _toast: ToastService,
  ) {
    super();
  }

  ngOnInit() {
    this.token = this._route.snapshot.queryParamMap.get('token') || '';
    this.frmGroup = this._fb.group({
      newPassword: [null, [Validators.required, Validators.minLength(6)]],
      confirm: [null, [Validators.required]],
    });
  }

  togglePassword() { this.showPassword = !this.showPassword; }

  get noToken(): boolean { return !this.token; }

  onSubmit() {
    if (!this.validateForm(this.frmGroup)) return;
    const v = this.frmGroup.value;
    if (v.newPassword !== v.confirm) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Mật khẩu xác nhận không khớp');
      return;
    }
    this.isLoading = true;
    this._service.resetPassword({ token: this.token, newPassword: v.newPassword })
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this.done = true;
            this._toast.success(StatusResponseTitle.SUCCESS, rs.message);
            setTimeout(() => this._router.navigate(['/login']), 2500);
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }
}
