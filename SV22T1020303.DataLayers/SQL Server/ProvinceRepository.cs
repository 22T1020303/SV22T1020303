using Dapper;
using Microsoft.Data.SqlClient;
using LiteCommerce.Models.DataDictionary;
using LiteCommerce.DataLayers.Interfaces;

namespace LiteCommerce.DataLayers.SQLServer
{
    /// <summary>
    /// Thực hiện các thao tác truy xuất dữ liệu liên quan đến bảng Provinces
    /// </summary>
    public class ProvinceRepository : IDataDictionaryRepository<Province>
    {
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo ProvinceRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server</param>
        public ProvinceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tỉnh/thành trong bảng Provinces
        /// </summary>
        /// <returns>Danh sách các tỉnh/thành</returns>
        public async Task<List<Province>> ListAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT ProvinceName
                           FROM Provinces
                           ORDER BY ProvinceName";

            var data = await connection.QueryAsync<Province>(sql);

            return data.ToList();
        }
    }
}