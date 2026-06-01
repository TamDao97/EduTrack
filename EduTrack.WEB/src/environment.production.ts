/**
 * Production environment — chỉnh apiUrl sau khi BE deploy lên Render.
 *
 * Sau khi Render assign URL (vd https://edutrack-api-xxxx.onrender.com):
 *   1. Thay apiUrl + fileUrl ở đây
 *   2. Commit + push → Vercel auto-redeploy
 *   3. Cập nhật `Cors__AllowedOrigins` trên Render dashboard để cho phép FE Vercel call BE
 */
export const environment = {
    production: true,
    apiUrl: 'https://edutrack-api.onrender.com/api',
    fileUrl: 'https://edutrack-api.onrender.com',
};
