import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class TutorProfileService {
  apiUrl = `${environment.apiUrl}/tutor-profile`;
  constructor(private _http: HttpClient) {}

  getMyProfile(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-my-profile`);
  }

  saveMyProfile(payload: any): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/save-my-profile`, payload);
  }
}
