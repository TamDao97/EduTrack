import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IResponse } from '../../shared/interfaces/IResponse';
import { TdBaseService } from '../../shared/utils/services/td-base.service';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root'
})
export class ConfigJsonService extends TdBaseService {
  override apiUrl: string = `${environment.apiUrl}/config-json`;
  constructor(_httpClient: HttpClient) {
    super(_httpClient);
  }

  getOrgConfig(): Observable<IResponse> {
    return this._httpClient.get<IResponse>(`${this.apiUrl}/get-org-config`);
  }

  saveOrgConfig(payload: any): Observable<IResponse> {
    return this._httpClient.post<IResponse>(`${this.apiUrl}/save-org-config`, payload);
  }
}
