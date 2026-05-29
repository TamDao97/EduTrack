import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IResponse } from '../../shared/interfaces/IResponse';
import { TdBaseService } from '../../shared/utils/services/td-base.service';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root',
})
export class UserService extends TdBaseService {
  override apiUrl: string = `${environment.apiUrl}/user`;
  constructor(_httpClient: HttpClient) {
    super(_httpClient);
  }

  getCurrentUser(): Observable<IResponse> {
    return this._httpClient.get<IResponse>(`${this.apiUrl}/get-current-user`);
  }

  getUserProfile(): Observable<IResponse> {
    return this._httpClient.get<IResponse>(`${this.apiUrl}/get-user-profile`);
  }

  saveUserProfile(payload: any): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/save-user-profile`, payload);
  }

  changePassword(payload: any): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/change-password`, payload);
  }
}
