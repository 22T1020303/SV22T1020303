using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.Catalog;
using LiteCommerce.Models.Common;
using LiteCommerce.DataLayers.Interfaces;
using System.Data;

namespace LiteCommerce.DataLayers.SQLServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            // Xử lý giá trị tìm kiếm
            string searchValue = string.IsNullOrEmpty(input.SearchValue) ? "" : input.SearchValue;

            // 1. Câu lệnh SQL đếm tổng số dòng (phải có lọc theo Category và Supplier)
            string countSql = @"SELECT COUNT(*)
                                FROM Products
                                WHERE (@SearchValue = N'' OR ProductName LIKE '%' + @SearchValue + '%')
                                  AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                  AND (@SupplierID = 0 OR SupplierID = @SupplierID)";

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                SearchValue = searchValue,
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID
            });

            // 2. Câu lệnh SQL lấy dữ liệu có phân trang và lọc
            string sql = @"SELECT *
                           FROM (
                               SELECT *, ROW_NUMBER() OVER (ORDER BY ProductName) AS RowNumber
                               FROM Products
                               WHERE (@SearchValue = N'' OR ProductName LIKE '%' + @SearchValue + '%')
                                 AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                 AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                           ) AS t
                           WHERE t.RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize";

            var data = await connection.QueryAsync<Product>(sql, new
            {
                SearchValue = searchValue,
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                Page = input.Page,
                PageSize = input.PageSize
            });

            return new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Products WHERE ProductID = @productID";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            // Lưu ý: Đã sửa IsSelling thành IsPublic theo ảnh cấu trúc bảng bạn gửi
            string sql = @"INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsPublic)
                           VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsPublic);
                           SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Products
                           SET ProductName=@ProductName, ProductDescription=@ProductDescription,
                               SupplierID=@SupplierID, CategoryID=@CategoryID,
                               Unit=@Unit, Price=@Price, Photo=@Photo, IsPublic=@IsPublic
                           WHERE ProductID=@ProductID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Products WHERE ProductID=@productID";
            int rows = await connection.ExecuteAsync(sql, new { productID });
            return rows > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(*) FROM OrderDetails WHERE ProductID=@productID";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });
            return count > 0;
        }

        // ================= ATTRIBUTE =================
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductAttributes WHERE ProductID=@productID ORDER BY DisplayOrder";
            var data = await connection.QueryAsync<ProductAttribute>(sql, new { productID });
            return data.ToList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductAttributes WHERE AttributeID=@attributeID";
            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
                           VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                           SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE ProductAttributes SET AttributeName=@AttributeName, AttributeValue=@AttributeValue, DisplayOrder=@DisplayOrder
                           WHERE AttributeID=@AttributeID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM ProductAttributes WHERE AttributeID=@attributeID";
            int rows = await connection.ExecuteAsync(sql, new { attributeID });
            return rows > 0;
        }

        // ================= PHOTO =================
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductPhotos WHERE ProductID=@productID ORDER BY DisplayOrder";
            var data = await connection.QueryAsync<ProductPhoto>(sql, new { productID });
            return data.ToList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductPhotos WHERE PhotoID=@photoID";
            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
                           VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                           SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE ProductPhotos SET Photo=@Photo, Description=@Description, DisplayOrder=@DisplayOrder, IsHidden=@IsHidden
                           WHERE PhotoID=@PhotoID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM ProductPhotos WHERE PhotoID=@photoID";
            int rows = await connection.ExecuteAsync(sql, new { photoID });
            return rows > 0;
        }
    }
}