import { Injectable } from '@angular/core';
import * as ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';

@Injectable({
  providedIn: 'root'
})
export class ExcelExportService {

  async exportToExcel<T extends object>(
    data: T[],
    fileName: string,
    sheetName: string = 'Sheet1',
    title?: string
  ): Promise<void> {
    if (!data || data.length === 0) {
      console.warn('Không có dữ liệu để xuất Excel');
      return;
    }

    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet(sheetName);

    let startRow = 1;

    if (title) {
      const titleRow = worksheet.addRow([title]);
      titleRow.font = { size: 16, bold: true };
      titleRow.alignment = { vertical: 'middle', horizontal: 'center' };
      worksheet.mergeCells(`A${startRow}:${String.fromCharCode(65 + Object.keys(data[0]).length - 1)}${startRow}`);
      startRow += 2;
    }

    const columns = Object.keys(data[0]).map(key => ({
      header: key,
      key,
      width: 25
    }));
    worksheet.columns = columns;

    const headerRow = worksheet.getRow(startRow);
    headerRow.values = columns.map(c => c.header);
    headerRow.font = { bold: true, color: { argb: 'FFFFFFFF' } };
    headerRow.fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FF007ACC' }
    };
    headerRow.alignment = { horizontal: 'center' };
    headerRow.height = 20;

    data.forEach((item: any) => {
      const rowValues = columns.map(col => item[col.key]);
      worksheet.addRow(rowValues);
    });

    worksheet.eachRow((row: any) => {
      row.alignment = { vertical: 'middle', wrapText: true };
      row.eachCell((cell: any) => {
        cell.border = {
          top: { style: 'thin', color: { argb: 'FF999999' } },
          left: { style: 'thin', color: { argb: 'FF999999' } },
          bottom: { style: 'thin', color: { argb: 'FF999999' } },
          right: { style: 'thin', color: { argb: 'FF999999' } }
        };
      });
    });

    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
    saveAs(blob, `${fileName}.xlsx`);
  }
}
