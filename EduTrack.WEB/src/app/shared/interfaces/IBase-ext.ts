/**
 * Extension cho các interface chung của Tutor domain.
 * Tách riêng IBase generic + IGridFilterBase để mọi entity tái sử dụng.
 */

export interface IBase {
  id?: string;
  dateCreated?: string;
  dateModify?: string;
  isDeleted?: boolean;
}

export interface IGridFilterBase {
  keyword?: string;
  pageNumber: number;
  pageSize: number;
}

export function defaultGridFilter(): IGridFilterBase {
  return { keyword: '', pageNumber: 1, pageSize: 20 };
}
