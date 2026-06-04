import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IStudentCourse } from '../../interfaces/IStudentCourse';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class StudentCourseService {
  apiUrl = `${environment.apiUrl}/student-course`;
  constructor(private _http: HttpClient) {}

  getByStudent(idStudent: string): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-by-student/${idStudent}`);
  }

  create(payload: IStudentCourse): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/create`, payload);
  }

  update(payload: IStudentCourse): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/update`, payload);
  }

  delete(id: string): Observable<IResponse> {
    return this._http.post<IResponse>(`${this.apiUrl}/delete/${id}`, null);
  }

  /** Danh sách MÔN distinct từ đăng ký môn (active) — cho dropdown lọc HS. */
  getSubjects(): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-subjects`);
  }
}
