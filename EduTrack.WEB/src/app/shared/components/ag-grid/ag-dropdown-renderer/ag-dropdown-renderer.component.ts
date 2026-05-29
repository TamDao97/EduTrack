import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { FormsModule } from '@angular/forms';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { CommonModule } from '@angular/common';

/**
 * Cell renderer hiển thị nz-select trực tiếp trong cell (always-rendered, không edit mode).
 *
 * cellRendererParams:
 *   options: IDropdown[]       — danh sách options [{ value, text }]
 *   valueField?: string        — default 'value'
 *   labelField?: string        — default 'text'
 *   onChange?: (data, newVal)  — callback khi user pick — để mark dirty
 */
@Component({
  selector: 'app-ag-dropdown-renderer',
  templateUrl: './ag-dropdown-renderer.component.html',
  styleUrls: ['./ag-dropdown-renderer.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, NzSelectModule],
})
export class AgDropdownRendererComponent implements ICellRendererAngularComp {
  value: any;
  options: any[] = [];
  valueField = 'value';
  labelField = 'text';

  private params!: ICellRendererParams;
  private onChangeCb?: (data: any, newVal: any) => void;
  private field!: string;

  agInit(params: ICellRendererParams & any): void {
    this.params = params;
    this.value = params.value;
    this.options = params.options || [];
    this.valueField = params.valueField || 'value';
    this.labelField = params.labelField || 'text';
    this.onChangeCb = params.onChange;
    this.field = params.colDef?.field || '';
  }

  /** ag-grid gọi khi cell value thay đổi từ ngoài (vd reload data) */
  refresh(params: ICellRendererParams): boolean {
    this.value = params.value;
    return true;
  }

  /** User pick option → cập nhật row data + báo dirty */
  onValueChange(newVal: any): void {
    this.value = newVal;
    if (this.params?.data && this.field) {
      this.params.data[this.field] = newVal;
    }
    this.onChangeCb?.(this.params.data, newVal);
  }
}
