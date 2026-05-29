import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable, map, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private _authService: AuthService) { }

  canActivate(): Observable<boolean | UrlTree> {
    //Xử lý call api lấy current user
    return this._authService.isLoggedIn().pipe(
      map(isLogin => {
        if (isLogin) {
          return true;
        } else {
          return this.router.createUrlTree(['/login']);
        }
      }),
      catchError(() => of(this.router.createUrlTree(['/login'])))
    );
  }
}
