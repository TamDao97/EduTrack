import { IDropdown } from '../interfaces/IDropdown';
import {
  GenderEnum,
  StatusEnum,
  CashTransactionTypeEnum,
  WageTypeEnum,
  CustomerOffAllocationTargetTypeEnums,
} from './enums';

export const DefaultCurrency = 'EUR';

export const LocalStorageKey = {
  Auth: 'auth',
  filter: 'filter',
};
export const StatusResponseTitle = {
  SUCCESS: 'Thành công',
  INFO: 'Thông báo',
  WARNING: 'Cảnh báo',
  ERROR: 'Lỗi',
};

export const StatusResponseMessage = {
  ADD_SUCCESS: 'Thêm mới thành công!',
  UPDATE_SUCCESS: 'Cập nhật thành công!',
  DELETE_SUCCESS: 'Xóa thành công!',
  WARNING_SYSTEM: 'Cảnh báo hệ thống',
  ERROR_SYSTEM: 'Lỗi hệ thống',
  INPUT_REQUIRED: 'Vui lòng nhập thông tin',
  NOT_SELECTED: 'Vui lòng tick chọn dữ liệu',
};

export const DateFormat = 'dd/MM/yyyy';

export const keyPage = {
  User: 'User',
  Page: 'Page',
  Role: 'Role',
  Line: 'Line',
  Supplier: 'Supplier',
  Brand: 'Brand',
  CommodityType: 'CommodityType',
  ShippingFeeSetup: 'ShippingFeeSetup',
  Giftcard: 'Giftcard',
  Customer: 'Customer',
  CashTransaction: 'CashTransaction',
  CashCategory: 'CashCategory',
  ExchangeRate: 'ExchangeRate',
  Bank: 'Bank',
  BankAccount: 'BankAccount',
  Order: 'Order',
  OrderLogistic: 'OrderLogistic',
  LineOff: 'LineOff',
  CustomerOff: 'CustomerOff',
  CustomerReceivable: 'CustomerReceivable',
  Invoice: 'Invoice',
  ShippingFee: 'ShippingFee',
  Reconciliation: 'Reconciliation',
  ReconciliationHistory: 'ReconciliationHistory',
  PurchaseReport: 'PurchaseReport',
  CargoReport: 'CargoReport',
  CargoReportV3: 'CargoReportV3',
  FlightRoute: 'FlightRoute',
  Customs: 'Customs',
  CustomsFeeSetup: 'CustomsFeeSetup',
  CommissionFeeSetup: 'CommissionFeeSetup',
  CashAccount: 'CashAccount',
};

export const PurchaseReportDataType = {
  ORD_SUPPLIER: 'ORD_SUPPLIER',
  ORD_GIFTCARD: 'ORD_GIFTCARD',
  GIFTCARD_DELIVERY: 'GIFTCARD_DELIVERY',
};

export const ListGender: IDropdown[] = [
  {
    value: GenderEnum.Male,
    text: 'Nam',
  },
  {
    value: GenderEnum.FeMale,
    text: 'Nữ',
  },
  {
    value: GenderEnum.Orther,
    text: 'Khác',
  },
];

export const ListIntStatus: IDropdown[] = [
  {
    value: StatusEnum.active,
    text: 'Đang hoạt động',
  },
  {
    value: StatusEnum.unactive,
    text: 'Dừng hoạt động',
  },
];

export const ListBooleanStatus: IDropdown[] = [
  {
    value: true,
    text: 'Đang hoạt động',
  },
  {
    value: false,
    text: 'Dừng hoạt động',
  },
];

export const ListWageType: IDropdown[] = [
  {
    value: WageTypeEnum.MoneyFixed,
    text: 'đ',
  },
  {
    value: WageTypeEnum.Percent,
    text: '%',
  },
];

export const ListTransactionType: IDropdown[] = [
  {
    value: CashTransactionTypeEnum.Receipt,
    text: 'Thu',
  },
  {
    value: CashTransactionTypeEnum.Payment,
    text: 'Chi',
  },
  {
    value: CashTransactionTypeEnum.Adjustment,
    text: 'Điều chỉnh',
  },
  {
    value: CashTransactionTypeEnum.Offset,
    text: 'Bù trừ',
  },
  {
    value: CashTransactionTypeEnum.InternalTransfer,
    text: 'Chuyển tiền nội bộ',
  },
];

export const ListCustomerOffAllocationTargetType: IDropdown[] = [
  {
    value: CustomerOffAllocationTargetTypeEnums.Order,
    text: 'Đơn hàng',
  },
  {
    value: CustomerOffAllocationTargetTypeEnums.GiftCard,
    text: 'Gift card',
  },
];
