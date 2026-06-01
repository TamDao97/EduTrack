import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IResponse } from '../../shared/interfaces/IResponse';
import { environment } from '../../../environment';

@Injectable({ providedIn: 'root' })
export class LoginService {
  private apiUrl = `${environment.apiUrl}/auth`;
  constructor(private httpClient: HttpClient) { }

  login(payload: any): Observable<IResponse> {
    return this.httpClient.post<IResponse>(`${this.apiUrl}/login`, payload);
  }

  /** Self-signup gia sư — auto-login với JWT trả về */
  signupTutor(payload: { userName: string; displayName: string; password: string; passwordConfirm: string }): Observable<IResponse> {
    return this.httpClient.post<IResponse>(`${this.apiUrl}/signup-tutor`, payload);
  }

  /** Gửi email reset password (link 1h). Luôn trả Success bất kể email có tồn tại — không leak. */
  forgotPassword(email: string): Observable<IResponse> {
    return this.httpClient.post<IResponse>(`${this.apiUrl}/forgot-password`, { email });
  }

  /** Xác nhận token + đặt mật khẩu mới. */
  resetPassword(payload: { token: string; newPassword: string }): Observable<IResponse> {
    return this.httpClient.post<IResponse>(`${this.apiUrl}/reset-password`, payload);
  }
}
