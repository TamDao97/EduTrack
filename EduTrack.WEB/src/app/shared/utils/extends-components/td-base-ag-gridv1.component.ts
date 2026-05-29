import { Component, OnInit } from '@angular/core';
import { ColDef } from 'ag-grid-community';

@Component({
  template: '',
})
export class TdBaseAgGridv1Component implements OnInit {
  totalPage: number = 0;
  totalRecord: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;
  pageSizeOptions = [
    this.pageSize,
    this.pageSize * 2,
    this.pageSize * 3,
    this.pageSize * 4,
    this.pageSize * 5,
  ];
  columnDefs: ColDef[] = [];
  defaultColDef: ColDef = {
    resizable: true,
    sortable: true,
    filter: true,
  };

  gridApi!: any;
  constructor() {}

  ngOnInit() {}

  /**
   * Lưu gridApi
   * @param params
   */
  onGridReady(params: any) {
    this.gridApi = params.api;
    // this.gridApi.setDatasource(this.createDatasource());
  }

  /**
   * Lấy data để gửi API,không dính summary
   * @returns
   */
  getGridData(): any[] {
    const data: any[] = [];
    this.gridApi.forEachNode((node: any) => {
      if (!node.rowPinned) {
        data.push(node.data);
      }
    });
    return data;
  }

  /**
   * CHỈ LẤY DATA ĐANG FILTER / SORT
   * @returns
   */
  getFilteredData(): any[] {
    const data: any[] = [];
    this.gridApi.forEachNodeAfterFilterAndSort((node: any) => {
      if (!node.rowPinned) {
        data.push(node.data);
      }
    });
    return data;
  }

  /**
   * Đánh dấu dòng dirty khi edit
   * @param params
   */
  onCellValueChanged(params: any) {
    params.data.__dirty = true;
    // this.calculateSummary();
  }

  /**
   * Lấy dòng dirty
   * @returns
   */
  getDirtyRows() {
    const data: any[] = [];
    this.gridApi.forEachNode((node: any) => {
      if (node.data?.__dirty) {
        data.push(node.data);
      }
    });
    return data;
  }
}
