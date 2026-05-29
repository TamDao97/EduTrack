import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IResponse } from '../../shared/interfaces/IResponse';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  private apiUrl = `${environment.apiUrl}/auth`;
  constructor(private httpClient: HttpClient) { }

  login(payload: any): Observable<IResponse> {
    return this.httpClient.post<IResponse>(`${this.apiUrl}/login`, payload);
  }
}
