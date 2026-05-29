import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environment';
import { Guid } from '../../utils/guid';
import { IResponse } from '../../interfaces/IResponse';
import { StatusCode } from '../../utils/enums';

@Component({
  selector: 'app-image-upload',
  templateUrl: './image-upload.component.html',
  styleUrls: ['./image-upload.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ImageUploadComponent),
      multi: true,
    },
  ],
})
export class ImageUploadComponent implements ControlValueAccessor {
  @Input() uploadUrl = `${environment.fileUrl}/api/file/upload`;
  @Input() deleteUrl = `${environment.fileUrl}/api/file/delete`;
  @Input() getByIdUrl = `${environment.fileUrl}/api/file/get-by-id`; // thêm api lấy file theo id

  idImage: Guid | null = null;
  imageUrl: string | null = null;
  isUploading = false;

  private onChange = (value: any) => { };
  private onTouched = () => { };

  constructor(
    private http: HttpClient,
    private msg: NzMessageService
  ) { }

  // khi bind lại dữ liệu từ form
  writeValue(value: string | null): void {
    this.idImage = value || null;
    if (this.idImage) {
      // gọi API getById để hiển thị lại ảnh
      this.http.get<IResponse>(`${this.getByIdUrl}/${this.idImage}`).subscribe({
        next: (res) => {
          if (res.status === StatusCode.Ok) {
            this.imageUrl = environment.fileUrl + res.data?.filePath;
          }
        },
        error: () => {
          this.imageUrl = null;
        },
      });
    } else {
      this.imageUrl = null;
    }
  }

  registerOnChange(fn: any): void { this.onChange = fn; }
  registerOnTouched(fn: any): void { this.onTouched = fn; }

  triggerFileInput(event: MouseEvent) {
    const input = (event.currentTarget as HTMLElement).querySelector('input');
    (input as HTMLInputElement).click();
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    this.isUploading = true;
    this.http.post<IResponse>(this.uploadUrl, formData).subscribe({
      next: (res) => {
        if (res.status === StatusCode.Ok) {
          this.idImage = res.data?.id;
          this.imageUrl = environment.fileUrl + res.data?.filePath;
          this.onChange(res.data?.id); // 👈 chỉ emit id
          this.msg.success('Upload thành công!');
        } else {
          this.msg.error('Upload thất bại!');
        }
      },
      error: () => this.msg.error('Upload thất bại!'),
      complete: () => (this.isUploading = false),
    });
  }

  removeImage(event: MouseEvent) {
    event.stopPropagation();
    if (!this.idImage) return;
    this.http.post<IResponse>(`${this.deleteUrl}/${this.idImage}`, null).subscribe({
      next: (res) => {
        if (res.status === StatusCode.Ok) {
          this.imageUrl = null;
          this.idImage = null;
          this.onChange(null);
          this.msg.info('Đã xoá ảnh.');
        } else {
          this.msg.error('Xóa ảnh thất bại!');
        }
      },
      error: () => this.msg.error('Xóa ảnh thất bại!'),
    });
  }
}
