namespace TD.Lib.Helper
{
    public class Utils
    {
        public static string HashPassword(string password)
        {
            // Sử dụng Bcrypt để mã hóa mật khẩu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            // Xác thực mật khẩu đã mã hóa với mật khẩu do người dùng cung cấp
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }

        public static string GenerateCode(string prefix)
        {
            long ticks = DateTime.UtcNow.Ticks; // 100-nanosecond units
            return $"{prefix}-{ticks}";
        }

        /// <summary>
        /// Quy đổi ngoại tệ sang VNĐ
        /// </summary>
        /// <param name="amount">Số tiền ngoại tệ</param>
        /// <param name="exchangeRate">Tỷ giá (vd: 1 USD = 25.200 VNĐ)</param>
        /// <param name="roundDigits">Số chữ số làm tròn (0 = VNĐ)</param>
        public static decimal ConvertToVND(decimal amount, decimal exchangeRate, int roundDigits = 0)
        {
            if (amount < 0) throw new ArgumentException("Số tiền phải >= 0");
            if (exchangeRate <= 0) throw new ArgumentException("Tỷ giá phải > 0");
            var result = amount * exchangeRate;
            return Math.Round(result, roundDigits, MidpointRounding.AwayFromZero);
        }
    }
}
