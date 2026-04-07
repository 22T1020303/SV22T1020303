using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace LiteCommerce.DataLayers.SQLServer
{
    public class EmployeeAccountRepository
    {
        private readonly string _connectionString;

        public EmployeeAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra đăng nhập nhân viên
        /// </summary>
        public async Task<bool> AuthorizeAsync(string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT COUNT(*) 
                            FROM Employees 
                            WHERE Email = @Email AND Password = @Password";

                var result = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    Email = email,
                    Password = password
                });

                return result > 0;
            }
        }

        /// <summary>
        /// Đổi mật khẩu nhân viên
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int employeeID, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"UPDATE Employees 
                            SET Password = @Password 
                            WHERE EmployeeID = @EmployeeID";

                var rows = await connection.ExecuteAsync(sql, new
                {
                    EmployeeID = employeeID,
                    Password = newPassword
                });

                return rows > 0;
            }
        }
    }
}