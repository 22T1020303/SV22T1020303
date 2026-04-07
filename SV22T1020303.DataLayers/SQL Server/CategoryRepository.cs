using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Catalog;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu liên quan đến bảng Categories
    /// trong cơ sở dữ liệu SQL Server.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo CategoryRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách loại hàng có phân trang và tìm kiếm
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả dữ liệu dạng phân trang</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Categories
                                WHERE CategoryName LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string dataSql = @"SELECT CategoryID, CategoryName, Description
                               FROM Categories
                               WHERE CategoryName LIKE @searchValue
                               ORDER BY CategoryName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<Category>(dataSql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một loại hàng theo CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>Thông tin loại hàng hoặc null nếu không tồn tại</returns>
        public async Task<Category?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT CategoryID, CategoryName, Description
                           FROM Categories
                           WHERE CategoryID = @id";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một loại hàng
        /// </summary>
        /// <param name="data">Thông tin loại hàng</param>
        /// <returns>Mã CategoryID vừa tạo</returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Categories
                           (CategoryName, Description)
                           VALUES
                           (@CategoryName, @Description);
                           SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="data">Dữ liệu loại hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Categories
                           SET CategoryName = @CategoryName,
                               Description = @Description
                           WHERE CategoryID = @CategoryID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa loại hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Categories
                           WHERE CategoryID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra loại hàng có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>True nếu đã được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE CategoryID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }
    }
}