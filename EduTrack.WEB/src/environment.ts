export const environment = {
    production: false,
    apiUrl: 'https://localhost:7246/api',
    fileUrl: 'https://localhost:7246',

    /** Thông tin nhận tiền thanh toán plan (founder). Đổi khi đi prod. */
    founder: {
        zaloPhone: '0987654321',     // SDT Zalo founder (tutor click để nhắn xác nhận)
        bankName: 'Techcombank',
        bankAccountNumber: '19038900001234',
        bankAccountHolder: 'NGUYEN VAN A',
        contactEmail: 'support@edutrack.vn',
    },
};
