// import { Pipe, PipeTransform } from '@angular/core';

// @Pipe({
//   name: 'currencyText',
//   standalone: true,
// })
// export class CurrencyTextPipe implements PipeTransform {

//   // Các đồng tiền không dùng số lẻ
//   private noFractionCurrencies = ['VND', 'JPY', 'KRW'];

//   /**
//    * Tự động tìm locale phù hợp với mã tiền tệ.
//    * Dò qua danh sách locale phổ biến.
//    * Nếu Intl hỗ trợ → lấy locale đó.
//    */
//   private getAutoLocale(currency: string): string {
//     const testLocales = [
//       'en-US', 'en-GB', 'en-CA', 'en-AU',
//       'vi-VN', 'ja-JP', 'zh-CN', 'zh-TW',
//       'ko-KR', 'th-TH', 'id-ID', 'ms-MY',
//       'de-DE', 'fr-FR', 'it-IT', 'nl-NL',
//       'es-ES', 'pt-PT', 'pt-BR', 'ru-RU',
//       'ar-AE', 'ar-SA', 'hi-IN', 'bn-BD',
//     ];

//     for (const locale of testLocales) {
//       try {
//         new Intl.NumberFormat(locale, {
//           style: 'currency',
//           currency,
//         });
//         return locale;
//       } catch { }
//     }

//     return 'en-US'; // fallback an toàn
//   }

//   transform(
//     value: number | string | null | undefined,
//     currencyCode: string = 'VND',
//     showSymbol: boolean = true,
//     fractionDigits: number | null = null,
//     locale: string | null = null,
//     negativeInParentheses: boolean = false
//   ): string {
//     if (value == null || value === '') return '';

//     // Loại bỏ dấu phẩy & khoảng trắng khi nhập chuỗi
//     const raw = String(value)
//       .replace(/,/g, '')
//       .replace(/\s/g, '');

//     const num = Number(raw);
//     if (isNaN(num)) return String(value);

//     // Locale tự động theo mã tiền
//     const useLocale = locale ?? this.getAutoLocale(currencyCode);

//     // Số thập phân auto nếu không truyền vào
//     const digits = fractionDigits ??
//       (this.noFractionCurrencies.includes(currencyCode) ? 0 : 2);

//     // Lấy giá trị tuyệt đối để xử lý âm
//     const absValue = Math.abs(num);

//     // -------------------
//     //  Trường hợp KHÔNG hiển thị ký hiệu tiền
//     // -------------------
//     if (!showSymbol) {
//       const formatted = new Intl.NumberFormat(useLocale, {
//         minimumFractionDigits: digits,
//         maximumFractionDigits: digits,
//       }).format(absValue);

//       if (num < 0) {
//         return negativeInParentheses ? `(${formatted})` : `-${formatted}`;
//       }

//       return formatted;
//     }

//     // -------------------
//     //  Format dạng tiền có ký hiệu
//     // -------------------
//     let formattedCurrency = '';

//     try {
//       formattedCurrency = new Intl.NumberFormat(useLocale, {
//         style: 'currency',
//         currency: currencyCode,
//         minimumFractionDigits: digits,
//         maximumFractionDigits: digits,
//       }).format(absValue);
//     } catch {
//       // fallback khi currencyCode sai
//       formattedCurrency = new Intl.NumberFormat(useLocale, {
//         minimumFractionDigits: digits,
//         maximumFractionDigits: digits,
//       }).format(absValue);
//     }

//     if (num < 0) {
//       return negativeInParentheses
//         ? `(${formattedCurrency})`
//         : `-${formattedCurrency}`;
//     }

//     return formattedCurrency;
//   }
// }
