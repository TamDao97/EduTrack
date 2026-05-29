import { Injectable } from '@angular/core';
import { TdBaseService } from '../../shared/utils/services/td-base.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IResponse } from '../../shared/interfaces/IResponse';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root',
})
export class RoleService extends TdBaseService {
  override apiUrl: string = `${environment.apiUrl}/role`;
  constructor(_httpClient: HttpClient) {
    super(_httpClient);
  }

  getPermissionGroupByModule(): Observable<IResponse> {
    return this._httpClient.get<IResponse>(
      `${this.apiUrl}/get-permission-by-module`
    );
  }

  getPermissionByRole(idRole: any): Observable<IResponse> {
    return this._httpClient.get<IResponse>(
      `${this.apiUrl}/get-permission-by-role/${idRole}`
    );
  }

  scanPermission(): Observable<IResponse> {
    return this._httpClient.post<IResponse>(
      `${this.apiUrl}/scan-permission`, null
    );
  }

  addPermissionByRole(payload: any): Observable<IResponse> {
    return this._httpClient.post<IResponse>(
      `${this.apiUrl}/add-permission-by-role`, payload
    );
  }
}
