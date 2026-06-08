import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IResponse } from '../../shared/interfaces/IResponse';

@Injectable({ providedIn: 'root' })
export class ReportService {
  apiUrl = `${environment.apiUrl}/tutor-report`;
  constructor(private _http: HttpClient) {}

  /** Báo cáo N tháng gần nhất (mặc định 6) — BE clamp 1..24 */
  getMyReport(months = 6): Observable<IResponse> {
    return this._http.get<IResponse>(`${this.apiUrl}/get-my-report?months=${months}`);
  }
}
