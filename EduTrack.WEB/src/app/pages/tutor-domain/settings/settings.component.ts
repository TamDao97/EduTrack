import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { TutorProfileService } from '../../../services/tutor-domain/tutor-profile.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { AuthService } from '../../../shared/utils/services/auth.service';
import { lookupBankBin } from '../../../shared/utils/vietqr';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
  standalone: true,
  imports: [SharedModule, CommonModule],
})
export class SettingsComponent extends TdBaseComponent implements OnInit {
  private _fb = inject(FormBuilder);
  private _toast = inject(ToastService);
  private _profileService = inject(TutorProfileService);
  private _router = inject(Router);

  frmGroup!: FormGroup;
  tutorDisplayName = '';
  isLoading = false;
  isSubmitting = false;

  ngOnInit() {
    const authStr = AuthService.getAuthStorage();
    if (authStr) {
      try { this.tutorDisplayName = JSON.parse(authStr).displayName || 'Gia sư'; } catch { this.tutorDisplayName = 'Gia sư'; }
    } else this.tutorDisplayName = 'Gia sư';

    this.initForm();
    this.loadProfile();
  }

  initForm() {
    this.frmGroup = this._fb.group({
      id: [null],
      idUser: [null],
      // Bank info — quan trọng nhất, vào group đầu
      bankName: [''],
      bankAccountNumber: [''],
      bankAccountHolder: [''],
      // Bio + subjects
      subjects: [''],
      bio: [''],
    });
  }

  loadProfile() {
    this.isLoading = true;
    this._profileService.getMyProfile()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(rs => {
        if (rs?.status === StatusCode.Ok && rs.data) {
          this.frmGroup.patchValue(rs.data);
        }
      });
  }

  /** Bank validation: nếu nhập bankName phải có accountNumber + accountHolder */
  validateBank(): boolean {
    const v = this.frmGroup.value;
    if (!v.bankName && !v.bankAccountNumber && !v.bankAccountHolder) return true; // tất cả rỗng OK
    if (!v.bankName || !v.bankAccountNumber || !v.bankAccountHolder) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Vui lòng nhập đủ cả 3 trường ngân hàng (Tên NH / Số TK / Chủ TK)');
      return false;
    }
    return true;
  }

  /** Cảnh báo nếu bankName không nhận diện được */
  get bankNotRecognized(): boolean {
    const name = this.frmGroup.value.bankName;
    if (!name || name.length < 3) return false;
    return !lookupBankBin(name);
  }

  onSave() {
    if (!this.validateBank()) return;
    this.isSubmitting = true;
    this._profileService.saveMyProfile(this.frmGroup.value)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok) {
            this._toast.success(StatusResponseTitle.SUCCESS, StatusResponseMessage.UPDATE_SUCCESS);
            // Patch để có id mới nếu vừa tạo lần đầu
            this.frmGroup.patchValue(rs.data || {});
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message);
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Lỗi hệ thống'),
      });
  }

  onLogout() {
    AuthService.logout();
    this._router.navigate(['/login']);
  }

  initials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    return (parts[0].charAt(0) + (parts.length > 1 ? parts[parts.length - 1].charAt(0) : '')).toUpperCase();
  }
}
