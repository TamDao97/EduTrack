import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IFeedback } from '../../interfaces/IFeedback';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  apiUrl = `${environment.apiUrl}/feedback`;
  constructor(private _http: HttpClient) {}

  /** Tutor gửi góp ý mới */
  create(payload: IFeedback): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/create`, payload);
  }

  /** Danh sách góp ý của chính tutor (kèm trạng thái + phản hồi founder) — paging */
  getMine(pageNumber = 1, pageSize = 10): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-mine?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  /** Admin: toàn bộ góp ý, lọc theo loại/trạng thái */
  getByFilter(filter: any): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/get-by-filter`, filter);
  }

  /** Admin: đổi trạng thái + ghi chú phản hồi */
  updateStatus(id: string, status: number, adminNote?: string | null): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/update-status/${id}`, { status, adminNote });
  }

  /** Admin: số góp ý "Mới" — badge nav */
  newCount(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/new-count`);
  }
}
