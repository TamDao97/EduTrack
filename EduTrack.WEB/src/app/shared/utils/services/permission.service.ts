import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { ICurrentUser } from '../../interfaces/ICurrentUser';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  private currentUser: ICurrentUser | null = null;
  constructor() {
    this.loadPermissions();
  }

  private loadPermissions() {
    const auth = AuthService.getAuthStorage();
    if (auth) {
      this.currentUser = JSON.parse(auth);
    }
  }

  getPermissions(): string[] {
    return this.currentUser?.permissions ?? [];
  }

  hasPermission(permission: string): boolean {
    return (this.currentUser?.isSuper ?? false) || this.getPermissions().includes(permission);
  }

  hasAnyPermission(perms: string[]): boolean {
    const userPerms = this.getPermissions();
    return (this.currentUser?.isSuper ?? false) || perms.some(p => userPerms.includes(p));
  }
}