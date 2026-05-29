import { Component, inject, Input, OnInit } from '@angular/core';
import { IControl } from '../../interfaces/IBase';
import { ControlTypeEnum } from '../../utils/enums';
import { SharedModule } from '../../modules/shared.module';
import { NZ_MODAL_DATA, NzModalRef } from 'ng-zorro-antd/modal';
import { StorageLocalService } from '../../utils/services/storage-local.service';
import { LocalStorageKey } from '../../utils/constants';

@Component({
  selector: 'app-filter-modal',
  templateUrl: './filter-modal.component.html',
  styleUrls: ['./filter-modal.component.scss'],
  standalone: true,
  imports: [SharedModule],
})
export class FilterModalComponent implements OnInit {
  params: any = inject(NZ_MODAL_DATA);
  modalRef: any = inject(NzModalRef);
  controls: IControl[] = [];
  controlTypeEnum = ControlTypeEnum;

  objFilter: any = {};
  keyPage: string = '';
  keyCache: string = '';
  constructor() { }

  ngOnInit() {
    this.keyPage = this.params.keyPage;
    this.keyCache = `${this.keyPage}${LocalStorageKey.filter}`;
    this.controls = this.params.controls as IControl[];
    this.objFilter = this.params.objFilter;
    this.initForm();
  }

  initForm() {
    const cacheValue = StorageLocalService.getItem(this.keyCache);
    const objValue = JSON.parse(cacheValue ?? '{}');
    this.controls.forEach((element: IControl) => {
      //ignore những biến filter đã có
      if (!this.objFilter[element.name]) {
        this.objFilter[element.name] = null; //objValue[element.name] ?? null;
      }
    });
  }

  onSearch(): void {
    StorageLocalService.setItem(this.keyCache, this.objFilter);
    this.modalRef.close(this.objFilter);
  }

  onClear(): void {
    this.controls.forEach((element: IControl) => {
      this.objFilter[element.name] = null;
    });
    StorageLocalService.removeItem(`${this.keyPage}${LocalStorageKey.filter}`);
  }

  onDateChange(e: any) { }

  onDateOk(e: any) { }

  onDateRangeChange(e: any) { }

  onDateRangeOk(result: Date | Date[] | null): void {
    console.log('onOk', result);
  }

  onCalendarChange(result: Array<Date | null>): void {
    console.log('onCalendarChange', result);
  }
}
