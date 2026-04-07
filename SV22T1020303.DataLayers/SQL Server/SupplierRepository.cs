using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Partner;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thực hiện các thao tác dữ liệu liên quan đến bảng Suppliers
    /// trong cơ sở dữ liệu SQL Server.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối CSDL
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm
        /// và phân trang dữ liệu.
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả dữ liệu dạng phân trang</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Suppliers
                                WHERE SupplierName LIKE @searchValue
                                OR ContactName LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string dataSql = @"SELECT *
                               FROM Suppliers
                               WHERE SupplierName LIKE @searchValue
                               OR ContactName LIKE @searchValue
                               ORDER BY SupplierName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<Supplier>(dataSql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin nhà cung cấp theo mã SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>Thông tin nhà cung cấp hoặc null nếu không tồn tại</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT *
                           FROM Suppliers
                           WHERE SupplierID = @id";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID của bản ghi vừa tạo</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Suppliers
                           (SupplierName, ContactName, Province, Address, Phone, Email)
                           VALUES
                           (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp
        /// </summary>
        /// <param name="data">Dữ liệu nhà cung cấp cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, ngược lại False</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Suppliers
                           SET SupplierName = @SupplierName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email
                           WHERE SupplierID = @SupplierID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa nhà cung cấp khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Suppliers
                           WHERE SupplierID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có đang được sử dụng
        /// trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>
        /// True nếu nhà cung cấp đang được sử dụng,
        /// False nếu chưa được sử dụng
        /// </returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE SupplierID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }
    }
}