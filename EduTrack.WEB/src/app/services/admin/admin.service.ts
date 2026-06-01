import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IAdminTutorFilter, IConfirmPaymentReq } from '../../interfaces/IAdmin';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class AdminService {
  apiUrl = `${environment.apiUrl}/admin`;
  constructor(private _http: HttpClient) { }

  getTutors(filter: IAdminTutorFilter): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/get-tutors`, filter);
  }

  confirmPayment(req: IConfirmPaymentReq): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/confirm-payment`, req);
  }

  getStats(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-stats`);
  }

  getPayments(filter: any): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/get-payments`, filter);
  }
}
