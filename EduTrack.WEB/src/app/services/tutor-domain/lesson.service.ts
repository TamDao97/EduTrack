import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { ILessonBulkCreateReq, ILessonGroupCreateReq } from '../../interfaces/ILesson';
import { IResponse } from '../../shared/interfaces/IResponse';
import { TdBaseService } from '../../shared/utils/services/td-base.service';

@Injectable({ providedIn: 'root' })
export class LessonService extends TdBaseService {
  override apiUrl = `${environment.apiUrl}/lesson`;
  constructor(httpClient: HttpClient) { super(httpClient); }

  getWeek(weekStart: string): Observable<IResponse> {
    return this._httpClient.get<IResponse>(`${this.apiUrl}/get-week?weekStart=${weekStart}`);
  }

  bulkCreateRecurring(payload: ILessonBulkCreateReq): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/bulk-create-recurring`, payload);
  }

  /** Tạo buổi nhóm — nhiều HS chung 1 ca, trả về số lesson đã tạo */
  createGroup(payload: ILessonGroupCreateReq): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/create-group`, payload);
  }

  markDone(id: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/mark-done/${id}`, {});
  }

  /** Đánh dấu "Đã dạy" hàng loạt mọi buổi đã qua giờ — trả về số buổi đã đánh dấu. */
  markDonePast(): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/mark-done-past`, {});
  }

  cancel(id: string, reason?: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/cancel/${id}`, { reason });
  }
}
