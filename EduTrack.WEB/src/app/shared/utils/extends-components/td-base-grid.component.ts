import { Component, inject, Type } from '@angular/core';
import { TdBaseComponent } from './td-base.component';
import { IPagingData } from '../../interfaces/IPagingData';
import { StatusCode } from '../enums';
import { TdBaseService } from '../services/td-base.service';
import { ToastService } from '../../services/toast.service';
import { StatusResponseTitle } from '../constants';
import { IColumn } from '../../interfaces/IBase';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  template: '',
})
export class TdBaseGridComponent extends TdBaseComponent {
  title: string = '';
  pageKey: string = '';
  gridColumns: IColumn[] = [];
  gridData: any[] = [];
  totalPage: number = 0;
  totalRecord: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;
  pageSizeOptions = [this.pageSize, this.pageSize * 2, this.pageSize * 3, this.pageSize * 4, this.pageSize * 5]
  isLoading: boolean = false;
  objFilter: any = {};

  // Checkbox
  checked = false;
  indeterminate = false;
  setOfCheckedId = new Set<string>();

  // //inject ở đây tránh đưa vào contructor phải đi sửa các class extends
  // public _spinner = inject(NgxSpinnerService);
  constructor(
    public _tdBaseService: TdBaseService,
    public _toastService: ToastService,
  ) {
    super();
  }

  ngOnInit() {
    // this.gridLoadData();
  }

  gridOptions(): any {
    return {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
    };
  }

  gridLoadData(filter: any): void {
    let objFilter = {
      ...filter,
      ...this.gridOptions(),
    };
    this.isLoading = true;
    this._spinner.show();
    this._tdBaseService.gridLoadData(objFilter).subscribe(
      (rs) => {
        if (rs.status == StatusCode.Ok) {
          const obj = rs.data as IPagingData;
          this.gridData = obj.data;
          this.totalRecord = obj.totalRecord;
          this.totalPage = obj.totalPage;
          this.pageNumber = obj.pageNumber;
        } else {
          this._toastService.error(StatusResponseTitle.ERROR, rs.message);
        }
      },
      (error) => {
        this._toastService.error(StatusResponseTitle.ERROR, error.message);
      },
      () => {
        // Khi hoàn thành
        this.isLoading = false;
        setTimeout(() => {
          this._spinner.hide();
        }, 3000);
      }
    );
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.gridLoadData(this.objFilter);
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.pageNumber = 1; // reset về trang đầu khi đổi page size
    this.gridLoadData(this.objFilter);
  }

  // // Pin column
  // getLeftOffset(index: number): string {
  //   let offset = 60; // width cột STT

  //   for (let i = 0; i < index; i++) {
  //     if (this.gridColumns[i].fixed === 'left') {
  //       offset += parseInt(this.gridColumns[i].width || '0', 10);
  //     }
  //   }
  //   return offset + 'px';
  // }

  // Checkbox
  updateCheckedSet(id: string, checked: boolean): void {
    if (checked) {
      this.setOfCheckedId.add(id);
    } else {
      this.setOfCheckedId.delete(id);
    }
  }

  onItemChecked(id: string, checked: boolean): void {
    this.updateCheckedSet(id, checked);
    this.refreshCheckedStatus();
  }

  onAllChecked(value: boolean): void {
    this.gridData.forEach((item) =>
      this.updateCheckedSet(item.id, value)
    );
    this.refreshCheckedStatus();
  }

  refreshCheckedStatus(): void {
    this.checked = this.gridData.every((item) =>
      this.setOfCheckedId.has(item.id)
    );
    this.indeterminate =
      this.gridData.some((item) =>
        this.setOfCheckedId.has(item.id)
      ) && !this.checked;
  }

  onCurrentPageDataChange($event: any): void {
    this.gridData = $event;
    this.refreshCheckedStatus();
  }
}
