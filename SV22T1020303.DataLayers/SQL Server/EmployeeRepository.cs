using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.HR;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác dữ liệu liên quan đến bảng Employees
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        /// <summary>
        /// Chuỗi kết nối đến SQL Server
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo EmployeeRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối database</param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy danh sách nhân viên có phân trang và tìm kiếm
        /// </summary>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Employees
                                WHERE FullName LIKE @searchValue
                                   OR Phone LIKE @searchValue
                                   OR Email LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string dataSql = @"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking
                               FROM Employees
                               WHERE FullName LIKE @searchValue
                                  OR Phone LIKE @searchValue
                                  OR Email LIKE @searchValue
                               ORDER BY FullName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<Employee>(dataSql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin 1 nhân viên theo EmployeeID
        /// </summary>
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Photo, IsWorking
                           FROM Employees
                           WHERE EmployeeID = @id";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }

        /// <summary>
        /// Thêm nhân viên mới
        /// </summary>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Employees
                           (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking)
                           VALUES
                           (@FullName,@BirthDate,@Address,@Phone,@Email,@Photo,@IsWorking);
                           SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Employees
                           SET FullName=@FullName,
                               BirthDate=@BirthDate,
                               Address=@Address,
                               Phone=@Phone,
                               Email=@Email,
                               Photo=@Photo,
                               IsWorking=@IsWorking
                           WHERE EmployeeID=@EmployeeID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Employees
                           WHERE EmployeeID=@id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra nhân viên có được sử dụng trong Orders không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE EmployeeID=@id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        /// <summary>
        /// Kiểm tra email của nhân viên có hợp lệ (không trùng) hay không
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
                        FROM Employees
                        WHERE Email=@email";
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Employees
                        WHERE Email=@email
                        AND EmployeeID<>@id";
            }

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

            return count == 0;
        }
    }
}