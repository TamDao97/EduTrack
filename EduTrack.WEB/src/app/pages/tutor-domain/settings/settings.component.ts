import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../../environment';
import { IResponse } from '../../../shared/interfaces/IResponse';
import { TutorProfileService } from '../../../services/tutor-domain/tutor-profile.service';
import { SharedModule } from '../../../shared/modules/shared.module';
import { ToastService } from '../../../shared/services/toast.service';
import { StatusResponseMessage, StatusResponseTitle } from '../../../shared/utils/constants';
import { StatusCode } from '../../../shared/utils/enums';
import { TdBaseComponent } from '../../../shared/utils/extends-components/td-base.component';
import { AuthService } from '../../../shared/utils/services/auth.service';
import { BANK_OPTIONS, lookupBankBin } from '../../../shared/utils/vietqr';

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

  bankOptions = BANK_OPTIONS;
  frmGroup!: FormGroup;
  isUploadingAvatar = false;
  fileUrl = environment.fileUrl;
  private _http = inject(HttpClient);
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
      avatarUrl: [null],
      // Bank info — quan trọng nhất, vào group đầu
      bankName: [''],
      bankAccountNumber: [''],
      bankAccountHolder: [''],
      // Bio + subjects
      subjects: [''],
      bio: [''],
    });
  }

  /** Url đầy đủ để render avatar — null nếu chưa có. */
  get avatarSrc(): string | null {
    const path = this.frmGroup?.value?.avatarUrl;
    return path ? `${this.fileUrl}${path}` : null;
  }

  /** User pick file → upload qua /api/file/upload → patch avatarUrl bằng filePath. */
  onAvatarPick(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (!/image\/(png|jpe?g|gif|webp)/i.test(file.type)) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Vui lòng chọn file ảnh (PNG/JPG/GIF)');
      input.value = '';
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this._toast.warning(StatusResponseTitle.WARNING, 'Ảnh quá 2MB — nén lại trước khi upload');
      input.value = '';
      return;
    }

    const formData = new FormData();
    formData.append('file', file);
    this.isUploadingAvatar = true;
    this._http.post<IResponse>(`${environment.apiUrl}/file/upload`, formData)
      .pipe(finalize(() => { this.isUploadingAvatar = false; input.value = ''; }))
      .subscribe({
        next: rs => {
          if (rs.status === StatusCode.Ok && rs.data?.filePath) {
            this.frmGroup.patchValue({ avatarUrl: rs.data.filePath });
            this._toast.success(StatusResponseTitle.SUCCESS, 'Upload ảnh thành công — nhớ bấm Lưu');
          } else {
            this._toast.error(StatusResponseTitle.ERROR, rs.message || 'Upload thất bại');
          }
        },
        error: () => this._toast.error(StatusResponseTitle.ERROR, 'Upload thất bại'),
      });
  }

  removeAvatar() {
    this.frmGroup.patchValue({ avatarUrl: null });
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
