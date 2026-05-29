import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { CommonModule } from '@angular/common';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';

/**
 * Cell renderer hiển thị value + nút copy → clipboard.
 *
 * Usage:
 *   { field: 'invoiceCode', cellRenderer: AgCopyableCellComponent }
 *
 * cellRendererParams (optional):
 *   showIfRowIndexInInvoiceFirst: boolean — chỉ hiển thị value ở row đầu invoice (cho grid V3 merge logic)
 */
@Component({
  selector: 'app-ag-copyable-cell',
  templateUrl: './ag-copyable-cell.component.html',
  styleUrls: ['./ag-copyable-cell.component.css'],
  standalone: true,
  imports: [CommonModule, NzIconModule, NzToolTipModule],
})
export class AgCopyableCellComponent implements ICellRendererAngularComp {
  value: any = null;
  copied = false;

  agInit(params: ICellRendererParams & any): void {
    this.applyParams(params);
  }

  refresh(params: ICellRendererParams & any): boolean {
    this.applyParams(params);
    return true;
  }

  private applyParams(params: ICellRendererParams & any): void {
    const showOnlyFirstInvoiceRow = params.showIfRowIndexInInvoiceFirst;
    if (showOnlyFirstInvoiceRow) {
      const rowIdx = params.data?.rowIndexInInvoice ?? 1;
      this.value = rowIdx === 1 ? params.value : null;
    } else {
      this.value = params.value;
    }
  }

  onCopy(event: MouseEvent): void {
    event.stopPropagation();
    if (this.value == null || this.value === '') return;
    const text = String(this.value);
    if (navigator?.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(() => this.flashCopied());
    } else {
      // Fallback cho browser cũ
      const ta = document.createElement('textarea');
      ta.value = text;
      ta.style.position = 'fixed';
      ta.style.opacity = '0';
      document.body.appendChild(ta);
      ta.select();
      try { document.execCommand('copy'); this.flashCopied(); } catch { /* ignore */ }
      document.body.removeChild(ta);
    }
  }

  private flashCopied(): void {
    this.copied = true;
    setTimeout(() => { this.copied = false; }, 1500);
  }
}
