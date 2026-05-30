import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IResponse } from '../../shared/interfaces/IResponse';
import { TdBaseService } from '../../shared/utils/services/td-base.service';

@Injectable({ providedIn: 'root' })
export class TuitionPeriodService extends TdBaseService {
  override apiUrl = `${environment.apiUrl}/tuitionperiod`;
  constructor(httpClient: HttpClient) { super(httpClient); }

  /** Lấy kỳ tháng-năm của HS (tự tạo nếu chưa có) */
  openOrGet(idStudent: string, month: number, year: number): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/open-or-get`, { idStudent, month, year });
  }

  /** Xem trước tổng tiền + danh sách buổi trước khi chốt */
  preview(idStudent: string, month: number, year: number): Observable<IResponse> {
    return this._httpClient.get<IResponse>(
      `${this.apiUrl}/preview?idStudent=${idStudent}&month=${month}&year=${year}`
    );
  }

  /** Chốt kỳ: lock các Lesson Done vào kỳ này, tính tổng */
  close(id: string, adjustment: number, notes?: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/close/${id}`, { adjustment, notes });
  }

  /** Ghi nhận thanh toán (full hoặc partial) */
  recordPayment(id: string, amount: number, notes?: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/record-payment/${id}`, { amount, notes });
  }
}
