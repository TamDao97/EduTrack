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

// Convert client time (giờ máy) → UTC (chuẩn lưu server)
export function toServerTime(date?: Date | null | undefined): Date | null {
  if (!date) return null;
  date = new Date(date);
  const offsetMinutes = date.getTimezoneOffset();
  return new Date(date.getTime() - offsetMinutes * 60000);
}

/**
 * Trả về 00:00:00.000 của ngày đó (giờ local).
 * Dùng cho cận dưới của filter date-range — đảm bảo include record từ đầu ngày.
 */
export function startOfDay(d?: Date | string | null): Date | null {
  if (!d) return null;
  const x = new Date(d);
  x.setHours(0, 0, 0, 0);
  return x;
}

/**
 * Trả về 23:59:59.999 của ngày đó (giờ local).
 * Dùng cho cận trên của filter date-range — đảm bảo include cả record cuối ngày.
 *
 * Bug tránh: nếu chỉ truyền date không có time, BE compare SellDate <= 5/1 00:00
 * sẽ bỏ sót record có SellDate = 5/1 14:30. endOfDay fix triệt để.
 */
export function endOfDay(d?: Date | string | null): Date | null {
  if (!d) return null;
  const x = new Date(d);
  x.setHours(23, 59, 59, 999);
  return x;
}

/**
 * Convert date-range cho filter API:
 *   from → startOfDay → toServerTime (UTC)
 *   to   → endOfDay   → toServerTime (UTC)
 *
 * Nhận tuple [from, to] hoặc undefined. Trả { from, to } đã chuẩn hóa.
 */
export function toServerDateRange(range?: [Date | null, Date | null] | null): {
  from: Date | null;
  to: Date | null;
} {
  if (!range) return { from: null, to: null };
  return {
    from: range[0] ? toServerTime(startOfDay(range[0])) : null,
    to: range[1] ? toServerTime(endOfDay(range[1])) : null,
  };
}

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