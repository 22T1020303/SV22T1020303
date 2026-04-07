using LiteCommerce.DataLayers.SQLServer;

namespace SV22T1020303.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng liên quan đến bảo mật cho hệ thống
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly EmployeeAccountRepository _userAccountDB;

        static SecurityDataService()
        {
            _userAccountDB = new EmployeeAccountRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập
        /// </summary>
        /// <param name="userName">Email</param>
        /// <param name="password">Mật khẩu đã mã hóa</param>
        /// <returns></returns>
        public static async Task<bool> AuthorizeAsync(string userName, string password)
        {
            return await _userAccountDB.AuthorizeAsync(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản
        /// </summary>
        /// <param name="employeeID">Mã nhân viên</param>
        /// <param name="password">Mật khẩu mới đã mã hóa</param>
        /// <returns></returns>
        public static async Task<bool> ChangePasswordAsync(int employeeID, string password)
        {
            return await _userAccountDB.ChangePasswordAsync(employeeID, password);
        }
    }
}