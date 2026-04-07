using Dapper;
using LiteCommerce.DataLayers.Interfaces;
using LiteCommerce.Models.Security;
using Microsoft.Data.SqlClient;

namespace LiteCommerce.DataLayers.SQLServer
{
    public class CustomerAccountRepository : ICustomerAccountRepository
    {
        private readonly string _connectionString;

        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            // Đã sửa: CustomerAccounts -> Customers
            var sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @Email";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<int> AddAsync(CustomerAccount account)
        {
            using var connection = new SqlConnection(_connectionString);
            // Đã sửa: CustomerAccounts -> Customers | PasswordHash -> Password
            var sql = @"INSERT INTO Customers(CustomerID, Email, [Password], IsLocked)
                        VALUES(@CustomerID, @Email, @PasswordHash, 0);
                        SELECT @CustomerID;";
            return await connection.ExecuteScalarAsync<int>(sql, account);
        }

        public async Task<CustomerAccount?> AuthorizeAsync(string email, string passwordHash)
        {
            using var connection = new SqlConnection(_connectionString);
            // Đã sửa: Lấy [Password] gán cho PasswordHash để khớp với Model C#
            var sql = @"SELECT CustomerID, Email, [Password] as PasswordHash, 
                               CASE WHEN IsLocked = 0 THEN 1 ELSE 0 END as IsActive
                        FROM Customers
                        WHERE Email = @Email
                          AND [Password] = @PasswordHash";

            return await connection.QueryFirstOrDefaultAsync<CustomerAccount>(sql, new
            {
                Email = email,
                PasswordHash = passwordHash
            });
        }

        public async Task<bool> ChangePasswordAsync(int customerId, string newPasswordHash)
        {
            using var connection = new SqlConnection(_connectionString);
            // Đã sửa: Dùng Procedure bạn đã tạo trong SQL
            var sql = "Account_ChangePassword";

            int rows = await connection.ExecuteAsync(sql, new
            {
                CustomerID = customerId,
                NewPassword = newPasswordHash // Khớp với tham số @NewPassword trong Procedure
            }, commandType: System.Data.CommandType.StoredProcedure);

            return rows > 0;
        }

        public async Task<CustomerAccount?> GetByCustomerIdAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            // Đã sửa: CustomerAccounts -> Customers | [Password] as PasswordHash
            var sql = @"SELECT CustomerID, Email, [Password] as PasswordHash,
                               CASE WHEN IsLocked = 0 THEN 1 ELSE 0 END as IsActive
                        FROM Customers
                        WHERE CustomerID = @CustomerID";
            return await connection.QueryFirstOrDefaultAsync<CustomerAccount>(sql, new
            {
                CustomerID = customerId
            });
        }
    }
}