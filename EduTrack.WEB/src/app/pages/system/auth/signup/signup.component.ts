import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { LoginService } from '../../../../services/system/login.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { ToastService } from '../../../../shared/services/toast.service';
import { StatusResponseTitle } from '../../../../shared/utils/constants';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';
import { AuthService } from '../../../../shared/utils/services/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, RouterModule],
})
export class SignupComponent extends TdBaseComponent {
  private _fb = inject(FormBuilder);
  private _router = inject(Router);
  private _toast = inject(ToastService);
  private _loginService = inject(LoginService);

  frmGroup: FormGroup = this._fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    userName: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    passwordConfirm: ['', [Validators.required]],
  });

  isSubmitting = false;
  showPassword = false;

  onSubmit() {
    if (!this.validateForm(this.frmGroup)) return;
    const v = this.frmGroup.value;
    if (v.password !== v.passwordConfirm) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Mật khẩu xác nhận không khớp');
      return;
    }
    this.isSubmitting = true;
    this._loginService.signupTutor(v)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            // Auto-login với JWT trả về
            AuthService.setAuthStorage(rs.data);
            this._toast.success(StatusResponseTitle.SUCCESS, 'Đăng ký thành công! Đang chuyển vào app…');
            setTimeout(() => this._router.navigate(['/dashboard']), 600);
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống, thử lại sau'),
      });
  }

  togglePassword() { this.showPassword = !this.showPassword; }
}
