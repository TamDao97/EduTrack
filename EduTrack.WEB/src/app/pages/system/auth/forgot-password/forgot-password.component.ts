import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { LoginService } from '../../../../services/system/login.service';
import { SharedModule } from '../../../../shared/modules/shared.module';
import { StatusCode } from '../../../../shared/utils/enums';
import { TdBaseComponent } from '../../../../shared/utils/extends-components/td-base.component';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule, RouterModule],
})
export class ForgotPasswordComponent extends TdBaseComponent implements OnInit {
  frmGroup!: FormGroup;
  isLoading = false;
  /** Sau khi submit thành công → ẩn form, hiện thông điệp "Đã gửi". */
  sent = false;
  sentEmail = '';

  constructor(private _fb: FormBuilder, private _service: LoginService) {
    super();
  }

  ngOnInit() {
    this.frmGroup = this._fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (!this.validateForm(this.frmGroup)) return;
    this.isLoading = true;
    const email = this.frmGroup.value.email.trim();
    this._service.forgotPassword(email)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(rs => {
        // BE luôn trả Ok cho dù email không tồn tại — không leak
        if (rs.status === StatusCode.Ok) {
          this.sent = true;
          this.sentEmail = email;
        }
      });
  }
}
