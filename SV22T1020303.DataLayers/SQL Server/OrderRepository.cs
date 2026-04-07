using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.DataLayers.Interfaces;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Sales;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác dữ liệu liên quan đến đơn hàng
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        /// <summary>
        /// Chuỗi kết nối database
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo OrderRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm danh sách đơn hàng có phân trang
        /// </summary>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            string searchValue = $"%{input.SearchValue}%";

            string countSql = @"SELECT COUNT(*)
                                FROM Orders O
                                LEFT JOIN Customers C ON O.CustomerID = C.CustomerID
                                WHERE C.CustomerName LIKE @searchValue";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                searchValue
            });

            string sql = @"SELECT O.*, C.CustomerName
                           FROM Orders O
                           LEFT JOIN Customers C ON O.CustomerID = C.CustomerID
                           WHERE C.CustomerName LIKE @searchValue
                           ORDER BY O.OrderTime DESC
                           OFFSET @offset ROWS
                           FETCH NEXT @pageSize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sql, new
            {
                searchValue,
                offset = input.Offset,
                pageSize = input.PageSize
            });

            return new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một đơn hàng
        /// </summary>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT O.*, C.CustomerName
                           FROM Orders O
                           LEFT JOIN Customers C ON O.CustomerID = C.CustomerID
                           WHERE O.OrderID=@orderID";

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        /// <summary>
        /// Thêm đơn hàng mới
        /// </summary>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Orders
                          (CustomerID,OrderTime,DeliveryProvince,DeliveryAddress,EmployeeID,
                           AcceptTime,ShipperID,ShippedTime,FinishedTime,Status)
                          VALUES
                          (@CustomerID,@OrderTime,@DeliveryProvince,@DeliveryAddress,@EmployeeID,
                           @AcceptTime,@ShipperID,@ShippedTime,@FinishedTime,@Status);
                          SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Orders
                           SET CustomerID=@CustomerID,
                               DeliveryProvince=@DeliveryProvince,
                               DeliveryAddress=@DeliveryAddress,
                               EmployeeID=@EmployeeID,
                               AcceptTime=@AcceptTime,
                               ShipperID=@ShipperID,
                               ShippedTime=@ShippedTime,
                               FinishedTime=@FinishedTime,
                               Status=@Status
                           WHERE OrderID=@OrderID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID=@orderID", new { orderID });

            int rows = await connection.ExecuteAsync("DELETE FROM Orders WHERE OrderID=@orderID", new { orderID });

            return rows > 0;
        }

        /// <summary>
        /// Lấy danh sách mặt hàng trong đơn hàng
        /// </summary>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT D.*, P.ProductName, P.Photo
                           FROM OrderDetails D
                           JOIN Products P ON D.ProductID = P.ProductID
                           WHERE D.OrderID=@orderID";

            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng trong đơn hàng
        /// </summary>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT D.*, P.ProductName, P.Photo
                           FROM OrderDetails D
                           JOIN Products P ON D.ProductID=P.ProductID
                           WHERE D.OrderID=@orderID AND D.ProductID=@productID";

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql,
                new { orderID, productID });
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng
        /// </summary>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO OrderDetails
                           (OrderID,ProductID,Quantity,SalePrice)
                           VALUES
                           (@OrderID,@ProductID,@Quantity,@SalePrice)";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán
        /// </summary>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE OrderDetails
                           SET Quantity=@Quantity,
                               SalePrice=@SalePrice
                           WHERE OrderID=@OrderID AND ProductID=@ProductID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"DELETE FROM OrderDetails
                           WHERE OrderID=@orderID AND ProductID=@productID";

            int rows = await connection.ExecuteAsync(sql, new { orderID, productID });

            return rows > 0;
        }
    }
}