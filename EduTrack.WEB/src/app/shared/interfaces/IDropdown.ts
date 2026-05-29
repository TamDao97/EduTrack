export interface IDropdown {
  value: any;
  text: string;
}

export interface IDropdownV2 extends IDropdown {
  code: string;
}

export interface IDropdownCustomer extends IDropdownV2 {
  fullName: string;
  address: string;
  phoneNumber: string;
  email: string;
  dateBirth: string;
  gender: string;
  wageType: number;
  wageValue: number;
  offPercent: number;
}

export interface IDropdownSupplier extends IDropdownV2 {
  Name: string;
  address: string;
  phoneNumber: string;
  email: string;
}

export interface IDropdownBankAccount extends IDropdown {
  bankName: string;
  accountName: string;
}

export interface IDropdownInvoice extends IDropdown {
  idLine: string;
  idSupplier: string;
  lineName: string;
  supplierName: string;
  notifyDate: Date;
  receivedDate: Date;
  kgSupplier: number;
  kgReceived: number;
  kgDifference: number;
}

export interface IDropdownCashCategory extends IDropdownV2 {
  partnerType: number;
}
