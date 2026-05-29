import { Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environment';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FileUploadComponent),
      multi: true,
    },
  ],
})
export class FileUploadComponent implements ControlValueAccessor {
  @Input() multiple = false;
  @Input() accept = ''; // ví dụ: '.jpg,.png,.pdf,.docx'
  @Input() maxFiles = 5;
  @Input() maxSizeMB = 5; // mặc định 5MB

  fileList: NzUploadFile[] = [];
  previewImage: string | null = null;
  previewVisible = false;
  isDisabled = false;

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private http: HttpClient, private msg: NzMessageService) {}

  writeValue(value: any): void {
    if (value && Array.isArray(value)) {
      this.fileList = value.map((f: any) => ({
        uid: f.id,
        name: f.fileName,
        url: f.url,
        status: 'done',
      }));
    } else {
      this.fileList = [];
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  // === Upload ===
  handleUpload = (item: NzUploadFile): boolean => {
    const fileSizeMB = item.size ? item.size / 1024 / 1024 : 0;

    if (fileSizeMB > this.maxSizeMB) {
      this.msg.error(`Kích thước file vượt quá ${this.maxSizeMB}MB`);
      return false;
    }

    const formData = new FormData();
    formData.append('file', item as any);

    this.http.post(`${environment.apiUrl}/file/upload`, formData).subscribe({
      next: (res: any) => {
        item.url = res.url;
        item.uid = res.id;
        item.status = 'done';
        this.msg.success(`Tải lên thành công: ${item.name}`);
        this.updateValue();
      },
      error: () => {
        item.status = 'error';
        this.msg.error(`Tải lên thất bại: ${item.name}`);
      },
    });

    return false;
  };

  // === Xóa file ===
  handleRemove = (file: NzUploadFile) => {
    if (file.uid) {
      this.http.delete(`${environment.apiUrl}/files/${file.uid}`).subscribe({
        next: () => this.msg.info('Đã xóa file.'),
        error: () => this.msg.error('Xóa file thất bại.'),
      });
    }
    this.fileList = this.fileList.filter((f) => f.uid !== file.uid);
    this.updateValue();
    return true;
  };

  // === Xem preview ảnh ===
  handlePreview = (file: NzUploadFile) => {
    if (file.url && this.isImage(file.name)) {
      this.previewImage = file.url;
      this.previewVisible = true;
    }
  };

  private updateValue() {
    const value = this.fileList
      .filter((f) => f.status === 'done')
      .map((f) => ({
        id: f.uid,
        fileName: f.name,
        url: f.url,
      }));
    this.onChange(value);
  }

  // === Helper ===
  formatSize(bytes?: number): string {
    if (!bytes) return '';
    const mb = bytes / 1024 / 1024;
    return mb < 1 ? `${(bytes / 1024).toFixed(1)} KB` : `${mb.toFixed(1)} MB`;
  }

  getFileIcon(name: string): string {
    const ext = name.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
        return 'file-image';
      case 'pdf':
        return 'file-pdf';
      case 'doc':
      case 'docx':
        return 'file-word';
      case 'xls':
      case 'xlsx':
        return 'file-excel';
      case 'zip':
      case 'rar':
        return 'file-zip';
      default:
        return 'file';
    }
  }

  isImage(name: string): boolean {
    return /\.(jpg|jpeg|png|gif)$/i.test(name);
  }
}
