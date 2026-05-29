export enum StatusCode {
  Ok = 200,
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  MethodNotAllowed = 405,
  InternalServerError = 500,
}

export enum ControlTypeEnum {
  Text,
  Select,
  SelectMultiple,
  Date,
  DateIgnoreTime,
  DateRange,
  DateRangeIgnoreTime,
  MonthYear,
  CheckBox,
  Radio,
  TextArea,
}

export enum GenderEnum {
  Male,
  FeMale,
  Orther,
}

export enum WageTypeEnum {
  Percent,
  MoneyFixed,
}

export enum StatusEnum {
  active,
  unactive,
}

export enum PaymentMethodTypeEnums {
  Cash = 1, //Tiền mặt
  BankTransfer = 2, //Chuyển khoản
}

export enum OrderPaymentMethodTypeEnums {
  // [Description("Tiền mặt")]
  Cash = 1, //Tiền mặt là nhà cung cấp ứng tiền sẽ hoàn trả ở phần công nợ với ncc
  // [Description("Thẻ GiftCard")]
  GiftCard = 2, //Thẻ GiftCard chưa đưa khách or đã đưa khách
  // [Description("Cả 2")]
  CashOrGiftCard = 3, //Cả 2 hình thức thanh toán
}

export enum ReconciliationEnums {
  // [Description("Kiểm đủ")]
  Matched = 1,
  // [Description("Kiểm thiếu")]
  Shortage = 2,
  // [Description("Kiểm thừa")]
  Excess = 3,
}

export enum GiftCardTypeEnums {
  // [Description("Đã giao khách")]
  Delivered = 1,
  // [Description("Chưa giao khách")]
  NotDelivered = 2,
}

export enum CashTransactionTypeEnum {
  Receipt = 1, // Thu
  Payment = 2, // Chi
  Adjustment = 3, //Điều chỉnh
  Offset = 4, //Bù trừ
  InternalTransfer = 5, //Chuyển tiền nội bộ
}

export enum PartnerTypeEnums {
  //  [Description("Không xác định")]
  None = 0,
  // [Description("Khách hàng")]
  Customer = 1,
  // [Description("Nhà cung cấp")]
  Supplier = 2,
  // [Description("Nhân viên")]
  Staff = 3,
  //Cargo = 3,
  //Internal = 4
}

export enum AllocationReferenceTypeEnums {
  Order = 1,
  GiftCard = 2,
  Shipping = 3,
  Other = 99,
}

export enum PaymentMethodEnums {
  Cash = 1,
  BankTransfer = 2,
  EWallet = 3,
}

export enum CustomerOffAllocationTargetTypeEnums {
  //  [Description("Đơn hàng")]
  Order = 1,
  //  [Description("Gift card")]
  GiftCard = 2,
}
