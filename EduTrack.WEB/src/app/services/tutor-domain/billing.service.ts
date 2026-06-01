import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IResponse } from '../../shared/interfaces/IResponse';

/**
 * Tutor xem subscription của chính mình + lịch sử thanh toán cá nhân.
 * Khác AdminService (founder-only). Endpoint: /api/subscription/*
 */
@Injectable({ providedIn: 'root' })
export class BillingService {
  apiUrl = `${environment.apiUrl}/subscription`;
  constructor(private _http: HttpClient) {}

  /** Lấy subscription + giá hiển thị của tutor đang login. Auto-tạo Trial nếu chưa có. */
  getMine(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-mine`);
  }

  /** Lịch sử các SubscriptionPayment đã confirm cho tutor đang login. */
  getMyPayments(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-my-payments`);
  }
}
