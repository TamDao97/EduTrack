import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environment';

@Injectable({ providedIn: 'root' })

export class ExcelService {
  apiUrl: string = `${environment.apiUrl}/excel`;
  constructor(private http: HttpClient) { }

  downloadTemplate() {
    return this.http.get(`${this.apiUrl}/template`, {
      responseType: 'blob' // bắt buộc để nhận file
    });
  }

  uploadExcel(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/import`, formData);
  }

}
