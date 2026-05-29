import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { TdBaseService } from '../../shared/utils/services/td-base.service';

@Injectable({ providedIn: 'root' })
export class ParentService extends TdBaseService {
  override apiUrl = `${environment.apiUrl}/parent`;
  constructor(httpClient: HttpClient) { super(httpClient); }
}
