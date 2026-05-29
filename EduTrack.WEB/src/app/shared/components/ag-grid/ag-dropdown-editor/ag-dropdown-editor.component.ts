import { Component } from '@angular/core';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { FormsModule } from '@angular/forms';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { CommonModule } from '@angular/common';

export const selectRenderer = (map: Record<string, string>) => {
  return (params: any) => map[params.value] || '';
};

@Component({
  selector: 'app-ag-dropdown-editor',
  templateUrl: './ag-dropdown-editor.component.html',
  styleUrls: ['./ag-dropdown-editor.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, NzSelectModule],
})
export class AgDropdownEditorComponent implements ICellEditorAngularComp {
  value: any;
  options: any[] = [];
  valueField = 'value';
  labelField = 'label';
  isOpen = true;  // mở dropdown ngay khi editor mount

  private params: any;

  agInit(params: any): void {
    this.params = params;
    this.value = params.value;
    this.options = params.options || [];
    this.valueField = params.valueField || 'value';
    this.labelField = params.labelField || 'label';
  }

  /** ag-grid gọi khi stop editing — trả về value mới để commit vào row */
  getValue() {
    return this.value;
  }

  /**
   * Khi dropdown đóng (user pick option HOẶC click outside) → commit value.
   * [(ngModel)] đã sync this.value với nz-select rồi nên getValue() sẽ trả đúng giá trị.
   */
  onOpenChange(open: boolean) {
    if (!open) {
      // setTimeout 0 — để Angular flush ngModel xong rồi mới stopEditing
      setTimeout(() => this.params?.api?.stopEditing(), 0);
    }
  }
}
