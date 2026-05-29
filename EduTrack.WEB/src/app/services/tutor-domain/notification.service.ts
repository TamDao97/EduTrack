import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { IResponse } from '../../shared/interfaces/IResponse';
import { TdBaseService } from '../../shared/utils/services/td-base.service';

@Injectable({ providedIn: 'root' })
export class NotificationService extends TdBaseService {
  override apiUrl = `${environment.apiUrl}/notification`;
  constructor(httpClient: HttpClient) { super(httpClient); }

  getInbox(): Observable<IResponse> {
    return this._httpClient.get<IResponse>(`${this.apiUrl}/get-inbox`);
  }

  markSent(id: string): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/mark-sent/${id}`, {});
  }
}
