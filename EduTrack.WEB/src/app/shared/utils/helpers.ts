import { ValidatorFn, AbstractControl, ValidationErrors } from "@angular/forms";

export function validateEmail(email: string): boolean {
  const re = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return re.test(String(email).toLowerCase());
}

export function confirmPassword(passwordControlName: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.parent) return null; // tránh lỗi khi chưa khởi tạo
    const password = control.parent.get(passwordControlName)?.value;
    const confirmPassword = control.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  };
}

// Lưu ý timezone: app lưu giờ VN local (xem BE AppTime). Filter ngày chỉ cần gửi
// chuỗi ngày local 'yyyy-MM-dd' (vd toISODate) — KHÔNG tự dịch offset sang UTC.
// (Đã bỏ toServerTime/toServerDateRange/startOfDay/endOfDay — hack UTC không còn dùng.)

// Chuẩn hóa currency về kiểu số để thực hiện tính toán
export function parseNumber(value: any): number {
  if (value == null || value === '') return 0;
  // Loại bỏ dấu phẩy, dấu cách, ký hiệu tiền
  const clean = value.toString().replace(/[^\d.-]/g, '');
  return parseFloat(clean);
}

// Convert ngoại tệ sang VNĐ
export function convertToVND(currency: number, rateVal: number): number {
  return currency * rateVal;
}