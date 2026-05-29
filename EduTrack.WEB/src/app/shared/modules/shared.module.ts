import { CommonModule } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTimePickerModule } from 'ng-zorro-antd/time-picker';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzCollapseModule } from 'ng-zorro-antd/collapse';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzTypographyModule } from 'ng-zorro-antd/typography';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { RouterModule } from '@angular/router';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { CurrencyFormatDirective } from '../directives/currency-fomat.directive';
// import { CurrencyTextPipe } from '../pipes/currency-text.pipe';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NzPopoverModule } from 'ng-zorro-antd/popover';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { ImageUploadComponent } from '../components/image-upload/image-upload.component';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { FileUploadComponent } from '../components/file-upload/file-upload.component';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { NzCascaderModule } from 'ng-zorro-antd/cascader';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { NzTimelineModule } from 'ng-zorro-antd/timeline';
import { NzProgressModule } from 'ng-zorro-antd/progress';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzStatisticModule } from 'ng-zorro-antd/statistic';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { EmptyDataComponent } from '../components/empty-data/empty-data.component';
import { HasPermissionDirective } from '../directives/has-permission.directive';

export const libModule = [
  NzFormModule,
  NzSelectModule,
  NzButtonModule,
  NzGridModule,
  NzDatePickerModule,
  NzTimePickerModule,
  NzInputModule,
  NzInputNumberModule,
  NzRadioModule,
  NzCardModule,
  NzCollapseModule,
  NzTableModule,
  NzTabsModule,
  NzTagModule,
  NzToolTipModule,
  NzTreeModule,
  NzDropDownModule,
  NzIconModule,
  NzModalModule,
  NzCheckboxModule,
  NzTypographyModule,
  NzPaginationModule,
  NzTreeSelectModule,
  NzSwitchModule,
  NgxSpinnerModule,
  NzPopoverModule,
  NzPopconfirmModule,
  NzUploadModule,
  NzSpinModule,
  NzAvatarModule,
  NzCascaderModule,
  NzDrawerModule,
  NzTimelineModule,
  NzProgressModule,
  NzSpaceModule,
  NzStatisticModule,
  NzDividerModule,
];
export const libDirective = [CurrencyFormatDirective, HasPermissionDirective];

export const libPipe = [
  // CurrencyTextPipe
];

export const customComponent = [
  ImageUploadComponent,
  FileUploadComponent,
  EmptyDataComponent,
];

@NgModule({
  declarations: [...libDirective, ...customComponent],
  imports: [
    RouterModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ...libModule,
  ],
  exports: [
    RouterModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ...libModule,
    ...libDirective,
    ...libPipe,
    ...customComponent,
  ],
})
export class SharedModule {}
