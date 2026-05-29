import { NzCheckBoxOptionInterface } from 'ng-zorro-antd/checkbox';
import { IDropdown } from './IDropdown';
import { ControlTypeEnum } from '../utils/enums';

export interface IControl {
  label?: string;
  placeHolder?: any;
  colClass: string;
  name: string;
  order: number;
  type: ControlTypeEnum;
  options?: IDropdown[];
  initValue?: any;
}

export interface IModalOptions {
  title: string;
  width: number;
  style?: any;
  className?: string;
}

export interface IColumn {
  field: string;
  header: string;
  width: string;
  class?: string;
  nzLeft?: true | false;
  nzRight?: true | false;
  sort: boolean;
  sortBy: 'asc' | 'desc';
  type:
  | 'text'
  | 'input'
  | 'date'
  | 'dateEditable'
  | 'daterange'
  | 'select'
  | 'checkbox'
  | 'radio'
  | 'textarea'
  | 'file'
  | 'image'
  | 'video'
  | 'audio'
  | 'currency'
  | 'currencyEditable'
  | 'template';
  customTemplate?: (data?: any) => any;
}
