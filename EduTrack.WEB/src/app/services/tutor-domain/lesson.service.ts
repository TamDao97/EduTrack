import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { ILessonBulkCreateReq } from '../../interfaces/ILesson';
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

  markDone(id: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/mark-done/${id}`, {});
  }

  cancel(id: string, reason?: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/cancel/${id}`, { reason });
  }
}
