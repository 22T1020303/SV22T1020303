using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Partner;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu liên quan đến bảng Customers
    /// trong cơ sở dữ liệu SQL Server.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng CustomerRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách khách hàng có phân trang và tìm kiếm
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả dữ liệu dạng phân trang</returns>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Customers
                                WHERE CustomerName LIKE @searchValue
                                OR ContactName LIKE @searchValue
                                OR Phone LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string dataSql = @"SELECT CustomerID, CustomerName, ContactName,
                                      Province, Address, Phone, Email, IsLocked
                               FROM Customers
                               WHERE CustomerName LIKE @searchValue
                               OR ContactName LIKE @searchValue
                               OR Phone LIKE @searchValue
                               ORDER BY CustomerName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<Customer>(dataSql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một khách hàng theo mã CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>Thông tin khách hàng hoặc null nếu không tồn tại</returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT CustomerID, CustomerName, ContactName,
                                  Province, Address, Phone, Email, IsLocked
                           FROM Customers
                           WHERE CustomerID = @id";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một khách hàng vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin khách hàng cần thêm</param>
        /// <returns>Mã khách hàng vừa tạo</returns>
        public async Task<int> AddAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Customers
                           (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                           VALUES
                           (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Customers
                           SET CustomerName = @CustomerName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email,
                               IsLocked = @IsLocked
                           WHERE CustomerID = @CustomerID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa khách hàng theo CustomerID
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Customers
                           WHERE CustomerID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra khách hàng có được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns>True nếu khách hàng đã được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE CustomerID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        /// <summary>
        /// Kiểm tra email có hợp lệ (không bị trùng) hay không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// id = 0: kiểm tra khi thêm mới  
        /// id ≠ 0: kiểm tra khi cập nhật
        /// </param>
        /// <returns>True nếu email hợp lệ</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql;

            if (id == 0)
            {
                sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @email";
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @email AND CustomerID <> @id";
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
            return count == 0;
        }
    }
}