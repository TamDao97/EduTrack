import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard chỉ cho phép super admin (founder) vào /admin. Tutor thường bị đẩy
 * về /dashboard tránh hit API 403 không cần thiết.
 */
@Injectable({ providedIn: 'root' })
export class SuperGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    const auth = AuthService.getAuthStorage();
    if (!auth) return this.router.createUrlTree(['/login']);
    try {
      const u = JSON.parse(auth);
      if (u?.isSuper) return true;
    } catch {}
    return this.router.createUrlTree(['/dashboard']);
  }
}
