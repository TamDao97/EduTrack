import { Injectable } from '@angular/core';
import { StorageLocalService } from './storage-local.service';
import { LocalStorageKey } from '../constants';
import { UserService } from '../../../services/system/user.service';
import { StatusCode } from '../enums';
import { Observable, map, catchError, of } from 'rxjs';
import { ICurrentUser } from '../../interfaces/ICurrentUser';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(
    private _router: Router,
    private _userService: UserService
  ) { }

  // Check login bằng cách call API
  isLoggedIn(): Observable<boolean> {
    return this._userService.getCurrentUser().pipe(
      map(rs => {
        if (rs.status === StatusCode.Ok) {
          //Mục đích là lấy thông tin mới nhất của user
          //Do trên server đang không dùng cache để lưu token nên phải thực hiện gán lại token lưu trên client 
          let authServerCacheObj = rs.data as ICurrentUser;
          let authLocalCache = AuthService.getAuthStorage();
          if (authLocalCache) {
            let authLocalCacheObj = JSON.parse(authLocalCache) as ICurrentUser;
            authServerCacheObj.accessToken = authLocalCacheObj.accessToken;
            authServerCacheObj.refreshToken = authLocalCacheObj.refreshToken;
          }
          AuthService.setAuthStorage(rs.data);
          return true;
        } else {
          AuthService.logout();
          return false;
        }
      }),
      catchError(() => {
        AuthService.logout();
        return of(false);
      })
    );
  }

  // Đăng xuất
  static logout(): void {
    //Xử lý call api để clear cache trên server
    StorageLocalService.removeItem(LocalStorageKey.Auth);
  }

  // Lưu token/user vào localStorage
  static setAuthStorage(item: any): void {
    StorageLocalService.setItem(LocalStorageKey.Auth, item);
  }

  //Get thông tin user
  static getAuthStorage(): string | null {
    return StorageLocalService.getItem(LocalStorageKey.Auth);
  }
}
