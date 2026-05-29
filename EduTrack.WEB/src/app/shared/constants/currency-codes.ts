// src/app/shared/constants/currency-codes.ts

export interface CurrencyCode {
  currencyCode: string;
  currencyName: string;
}

export const CURRENCY_CODES: CurrencyCode[] = [
  { currencyCode: 'AUD', currencyName: 'AUSTRALIAN DOLLAR' },
  { currencyCode: 'CAD', currencyName: 'CANADIAN DOLLAR' },
  { currencyCode: 'CHF', currencyName: 'SWISS FRANC' },
  { currencyCode: 'CNY', currencyName: 'YUAN RENMINBI' },
  { currencyCode: 'DKK', currencyName: 'DANISH KRONE' },
  { currencyCode: 'EUR', currencyName: 'EURO' },
  { currencyCode: 'GBP', currencyName: 'POUND STERLING' },
  { currencyCode: 'HKD', currencyName: 'HONGKONG DOLLAR' },
  { currencyCode: 'INR', currencyName: 'INDIAN RUPEE' },
  { currencyCode: 'JPY', currencyName: 'YEN' },
  { currencyCode: 'KRW', currencyName: 'KOREAN WON' },
  { currencyCode: 'KWD', currencyName: 'KUWAITI DINAR' },
  { currencyCode: 'MYR', currencyName: 'MALAYSIAN RINGGIT' },
  { currencyCode: 'NOK', currencyName: 'NORWEGIAN KRONER' },
  { currencyCode: 'RUB', currencyName: 'RUSSIAN RUBLE' },
  { currencyCode: 'SAR', currencyName: 'SAUDI RIAL' },
  { currencyCode: 'SEK', currencyName: 'SWEDISH KRONA' },
  { currencyCode: 'SGD', currencyName: 'SINGAPORE DOLLAR' },
  { currencyCode: 'THB', currencyName: 'THAILAND BAHT' },
  { currencyCode: 'USD', currencyName: 'US DOLLAR' },
];
