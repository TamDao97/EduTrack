import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IClassRoom } from '../../interfaces/IClassRoom';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class ClassRoomService {
  apiUrl = `${environment.apiUrl}/class-room`;
  constructor(private _http: HttpClient) {}

  getMyClasses(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-my-classes`);
  }

  /** Danh sách lớp có lọc (keyword/isActive) + paging — dùng cho load-more. */
  getByFilter(filter: { keyword: string; isActive: boolean | null; pageNumber: number; pageSize: number }): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/get-by-filter`, filter);
  }

  getDetail(id: string): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-detail/${id}`);
  }

  create(payload: IClassRoom): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/create`, payload);
  }

  update(payload: IClassRoom): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/update`, payload);
  }

  delete(id: string): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/delete/${id}`, null);
  }

  addMember(idClass: string, idStudent: string, rateOverride?: number | null): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/add-member`, { idClass, idStudent, rateOverride });
  }

  updateMember(idMember: string, rateOverride?: number | null): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/update-member/${idMember}`, { rateOverride });
  }

  removeMember(idMember: string): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/remove-member/${idMember}`, null);
  }

  /** Xếp lịch N tuần → trả số buổi đã sinh (idempotent — chạy lại không tạo trùng) */
  generateSchedule(idClass: string, startDate: string, numberOfWeeks: number): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/generate-schedule`, { idClass, startDate, numberOfWeeks });
  }

  /** Các lớp 1 HS đang ghi danh */
  getByStudent(idStudent: string): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-by-student/${idStudent}`);
  }
}
