/**
 * Production environment — chỉnh apiUrl sau khi BE deploy lên Render.
 *
 * Sau khi Render assign URL (vd https://edutrack-api-xxxx.onrender.com):
 *   1. Thay apiUrl + fileUrl ở đây
 *   2. Commit + push → Vercel auto-redeploy
 *   3. Cập nhật `Cors__AllowedOrigins` trên Render dashboard để cho phép FE Vercel call BE
 *   4. Đổi `founder` thật trước khi cho user đầu tiên thanh toán
 */
export const environment = {
    production: true,
    apiUrl: 'https://edutrack-api.onrender.com/api',
    fileUrl: 'https://edutrack-api.onrender.com',

    founder: {
        zaloPhone: '0987654321',          // SDT Zalo founder thật
        bankName: 'Techcombank',
        bankAccountNumber: '19038900001234',
        bankAccountHolder: 'NGUYEN VAN A',
        contactEmail: 'support@edutrack.vn',
    },
};
