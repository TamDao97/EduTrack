import {
  AfterViewInit,
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  output,
  SimpleChanges,
} from '@angular/core';
import { IControl } from '../../interfaces/IBase';
import { LocalStorageKey } from '../../utils/constants';
import { ControlTypeEnum } from '../../utils/enums';
import { StorageLocalService } from '../../utils/services/storage-local.service';
import { SharedModule } from '../../modules/shared.module';

@Component({
  selector: 'app-filter-modal-ver1',
  templateUrl: './filter-modal-ver1.component.html',
  styleUrl: './filter-modal-ver1.component.css',
  standalone: true,
  imports: [SharedModule],
})
export class FilterModalVer1Component implements OnInit, OnChanges {
  @Input() keyPage: string = '';
  @Input() filterControls: IControl[] = [];
  @Input() objFilter: any = {};
  @Output() searchOutput = new EventEmitter<any>();
  controlTypeEnum = ControlTypeEnum;

  keyCache: string = '';
  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    // if (changes['filterControls'] && this.filterControls?.length) {
    //   if (!this.objFilter) {
    //     this.objFilter = {};
    //   }
    //   this.filterControls.forEach(ctrl => {
    //     if (this.objFilter[ctrl.name] === undefined) {
    //       this.objFilter[ctrl.name] = ctrl.initValue ?? null;
    //     }
    //   });
    // }
  }

  ngOnInit() {
    this.keyCache = `${this.keyPage}${LocalStorageKey.filter}`;

    if (!this.objFilter) this.objFilter = {};
    this.filterControls.forEach((element: IControl) => {
      this.objFilter[element.name] = element?.initValue ?? null;
    });

    // AUTO SEARCH SAU KHI INIT XONG
    // KHÔNG CẦN GỌI HÀM Search, gridLoadData trong ngOnInit component cha nữa
    this.searchOutput.emit(this.objFilter);
    // console.log(this.objFilter);
  }

  onSearch(): void {
    StorageLocalService.setItem(this.keyCache, this.objFilter);
    this.searchOutput.emit(this.objFilter);
  }

  onClear(): void {
    this.filterControls.forEach((element: IControl) => {
      this.objFilter[element.name] = null;
    });
    StorageLocalService.removeItem(`${this.keyPage}${LocalStorageKey.filter}`);
    this.searchOutput.emit(this.objFilter);
  }
}
