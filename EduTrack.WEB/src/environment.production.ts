/**
 * Production environment — chỉnh apiUrl sau khi BE deploy lên Render.
 *
 * URL thật Render gán cho service edutrack-api (tên edutrack-api bị trùng global
 * nên có hậu tố -137i). Đổi ở đây nếu sau này tạo lại service với URL khác:
 *   1. Thay apiUrl + fileUrl
 *   2. Commit + push → edutrack-web tự build lại
 *   3. Cập nhật `Cors__AllowedOrigins` trên Render để cho phép edutrack-web call API
 *   4. Đổi `founder` thật trước khi cho user đầu tiên thanh toán
 */
export const environment = {
    production: true,
    apiUrl: 'https://edutrack-api-137i.onrender.com/api',
    fileUrl: 'https://edutrack-api-137i.onrender.com',

    founder: {
        zaloPhone: '0987654321',          // SDT Zalo founder thật
        bankName: 'Techcombank',
        bankAccountNumber: '19038900001234',
        bankAccountHolder: 'NGUYEN VAN A',
        contactEmail: 'support@edutrack.vn',
    },
};
