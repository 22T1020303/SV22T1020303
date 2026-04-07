using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Partner;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu liên quan đến bảng Shippers
    /// trong cơ sở dữ liệu SQL Server.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng ShipperRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách người giao hàng có phân trang và tìm kiếm
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả dữ liệu dạng phân trang</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Shippers
                                WHERE ShipperName LIKE @searchValue
                                OR Phone LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string dataSql = @"SELECT ShipperID, ShipperName, Phone
                               FROM Shippers
                               WHERE ShipperName LIKE @searchValue
                               OR Phone LIKE @searchValue
                               ORDER BY ShipperName
                               OFFSET @offset ROWS
                               FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<Shipper>(dataSql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một người giao hàng theo ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>Thông tin người giao hàng hoặc null nếu không tồn tại</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT ShipperID, ShipperName, Phone
                           FROM Shippers
                           WHERE ShipperID = @id";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một người giao hàng
        /// </summary>
        /// <param name="data">Thông tin người giao hàng</param>
        /// <returns>Mã ShipperID vừa tạo</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Shippers
                           (ShipperName, Phone)
                           VALUES
                           (@ShipperName, @Phone);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        /// <param name="data">Dữ liệu người giao hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Shippers
                           SET ShipperName = @ShipperName,
                               Phone = @Phone
                           WHERE ShipperID = @ShipperID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa người giao hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM Shippers
                           WHERE ShipperID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra người giao hàng có được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>True nếu đã được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE ShipperID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }
    }
}