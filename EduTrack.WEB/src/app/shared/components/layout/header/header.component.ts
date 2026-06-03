import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ICurrentUser } from '../../../interfaces/ICurrentUser';
import { AuthService } from '../../../utils/services/auth.service';
import { TdBaseComponent } from '../../../utils/extends-components/td-base.component';
import { UserProfileComponent } from '../../../../pages/system/auth/user/user-profile/user-profile.component';
import { UserChangePasswordComponent } from '../../../../pages/system/auth/user/user-change-password/user-change-password.component';
import { Router } from '@angular/router';
import { ConfigJsonComponent } from '../../../../pages/system/config-json/config-json.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class HeaderComponent extends TdBaseComponent implements OnInit {
  currentUser: ICurrentUser;

  constructor(private _router: Router) {
    super();
  }

  ngOnInit() {
    const auth = AuthService.getAuthStorage(); // Lấy token từ localStorage
    if (auth) {
      this.currentUser = JSON.parse(auth) as ICurrentUser;
    }
  }


  onSettings() {
    this.openModal(
      {
        title: 'Cài đặt hệ thống',
        width: 1200,
      },
      ConfigJsonComponent,
      {
        // params: { id: this.currentUser.id },
      }
    ).afterClose.subscribe((result: any) => {
      // console.log(result);
    });
  }

  onEditProfile() {
    this.openModal(
      {
        title: 'Cập nhật thông tin cá nhân',
        width: 800,
      },
      UserProfileComponent,
      {
        params: { id: this.currentUser.id },
      }
    ).afterClose.subscribe((result: any) => {
      // console.log(result);
    });
  }

  onChangePassword() {
    this.openModal(
      {
        title: 'Đổi mật khẩu',
        width: 800,
      },
      UserChangePasswordComponent
    ).afterClose.subscribe((result: any) => {
      // console.log(result);
    });
  }

  /** Founder (IsSuper) quay lại Platform Console từ workspace gia sư. */
  goToAdmin() {
    this._router.navigate(['/admin']);
  }

  onLogout() {
    AuthService.logout();
    this._router.navigate(['/login']);
  }
}
