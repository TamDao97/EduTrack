import { inject, Type } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { NzModalRef, NzModalService } from 'ng-zorro-antd/modal';
import { IModalOptions } from '../../interfaces/IBase';
import { NgxSpinnerService } from 'ngx-spinner';

export class TdBaseComponent {
  private _modal = inject(NzModalService); // inject không cần constructor
  private _modalRef = inject(NzModalRef, { optional: true }); // Tránh lỗi nếu không có provider

  //inject ở đây tránh đưa vào contructor phải đi sửa các class extends
  public _spinner = inject(NgxSpinnerService);
  constructor() { }

  /**
   * Validate form
   * @param frmGroup validate formGroup
   * @returns
   */
  validateForm(form: AbstractControl): boolean {
    if (form.valid) return true;
    if (form instanceof FormGroup || form instanceof FormArray) {
      Object.values(form.controls).forEach((control) => {
        this.validateForm(control); // Đệ quy cho control con
      });
    }
    form.markAsTouched({ onlySelf: true });
    form.markAsDirty({ onlySelf: true });
    form.updateValueAndValidity({ onlySelf: true });
    return false;
  }

  /**
   * Open modal
   * @param options
   * @param component
   * @param params
   * @returns
   */
  openModal<T>(
    options: IModalOptions,
    component: Type<T>,
    params: Partial<T> = {}
  ) {
    return this._modal.create({
      nzTitle: options.title,
      nzContent: component,
      nzData: params,
      nzClosable: true,
      nzMaskClosable: true,
      nzWidth: options.width,
      nzStyle: options.style,
      nzClassName: options.className || '',
      nzFooter: null, // hoặc bạn có thể custom footer
    });
  }

  /**
   * Close modal
   * @param data
   */
  closeModal(data?: any) {
    this._modalRef?.close(data);
  }

  /**
   * Confirm modal
   * @param message 
   * @param onOk 
   * @param title 
   */
  confirmModal(message: string, onOk: () => void, title: string = 'Xác nhận') {
    this._modal.confirm({
      nzTitle: title,
      nzContent: message,
      nzOkText: 'Đồng ý',
      nzCancelText: 'Huỷ',
      nzOkType: 'default',
      nzOnOk: onOk
    });
  }

  /**
   * 
   * @param value 
   * @returns 
   */
  convertCurrencyToNumber(value: any): number {
    if (!value) return 0;
    value = value + ''; //Đảm bảo convert về string
    // Chỉ giữ lại số và dấu '.'
    const cleaned = value.replace(/[^0-9.]/g, '');
    const num = parseFloat(cleaned);
    return isNaN(num) ? 0 : num;
  }
}
