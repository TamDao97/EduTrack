/**
 * VietQR image URL builder + bank BIN lookup.
 *
 * Dùng API img.vietqr.io (free, không cần auth).
 *   Format: https://img.vietqr.io/image/{BIN}-{ACCOUNT}-{TEMPLATE}.png
 *   Params: ?amount={n}&addInfo={text}&accountName={ChuTK}
 */

/** Mapping tên ngân hàng phổ biến → BIN code (TCKH). */
const BANK_BINS: Record<string, string> = {
  'vietcombank': '970436', 'vcb': '970436',
  'techcombank': '970407', 'tcb': '970407',
  'bidv':        '970418',
  'vpbank':      '970432', 'vp':  '970432',
  'tpbank':      '970423', 'tp':  '970423',
  'mb bank':     '970422', 'mbbank': '970422', 'mb': '970422', 'mb-bank': '970422',
  'acb':         '970416',
  'sacombank':   '970403', 'stb':    '970403',
  'agribank':    '970405',
  'vietinbank':  '970415', 'ctg':    '970415',
  'hdbank':      '970437',
  'msb':         '970426',
  'ocb':         '970448',
  'vib':         '970441',
  'shb':         '970443',
  'eximbank':    '970431',
  'seabank':     '970440',
  'lpbank':      '970449', 'lienvietpostbank': '970449',
  'cake':        '546034',
  'timo':        '963388',
  'momo':        'momo',
};

/** Trả về BIN từ tên ngân hàng (case-insensitive, fuzzy). Null nếu không match. */
export function lookupBankBin(bankName: string | undefined | null): string | null {
  if (!bankName) return null;
  const key = bankName.trim().toLowerCase().replace(/\s+/g, '');
  if (BANK_BINS[key]) return BANK_BINS[key];
  // Try partial match
  for (const [k, v] of Object.entries(BANK_BINS)) {
    if (key.includes(k.replace(/\s+/g, ''))) return v;
  }
  return null;
}

export interface VietQrParams {
  bankName: string;
  accountNumber: string;
  accountHolder?: string;
  amount?: number;
  addInfo?: string;
  template?: 'compact' | 'compact2' | 'qr_only' | 'print';
}

/**
 * Build URL ảnh VietQR. Trả null nếu thiếu thông tin hoặc bank không nhận diện.
 */
export function buildVietQrUrl(p: VietQrParams): string | null {
  const bin = lookupBankBin(p.bankName);
  if (!bin || !p.accountNumber) return null;
  const tpl = p.template || 'compact';
  const acc = p.accountNumber.trim();
  const params = new URLSearchParams();
  if (p.amount && p.amount > 0) params.set('amount', String(Math.round(p.amount)));
  if (p.addInfo)                params.set('addInfo', p.addInfo);
  if (p.accountHolder)          params.set('accountName', p.accountHolder);
  const qs = params.toString();
  return `https://img.vietqr.io/image/${bin}-${acc}-${tpl}.png${qs ? '?' + qs : ''}`;
}

/** Build text nội dung CK chuẩn: "HP <tên HS> T<tháng>" */
export function buildTransferContent(studentName: string, month: number, year: number): string {
  // Bỏ dấu để tránh ngân hàng escape
  const clean = removeVnAccents(studentName).trim().split(/\s+/).slice(-2).join(' ');
  return `HP ${clean} T${String(month).padStart(2, '0')}${String(year).slice(2)}`;
}

function removeVnAccents(s: string): string {
  return s.normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D');
}
