namespace SV22T1020303.BusinessLayers
{
    /// <summary>
    /// Lưu giữ các thông tin cấu hình cần sử dụng cho BusinessLayers
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";

        /// <summary>
        /// Khởi tạo cầu hình cho BusinessLayers
        /// (Hàm này phải được gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Thuộc tính trả về chuỗi tham số kết nối đến CSDL
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
