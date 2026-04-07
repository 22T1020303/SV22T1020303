using LiteCommerce.DataLayers.Interfaces;
using LiteCommerce.DataLayers.SQLServer;
using LiteCommerce.Models.Common;
using LiteCommerce.Models.Partner;
using System.Text.RegularExpressions;

namespace SV22T1020303.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng tác nghiệp (Business Logic) liên quan
    /// đến các đối tác trong hệ thống LiteCommerce.
    /// Các đối tác bao gồm:
    /// - Nhà cung cấp (Supplier)
    /// - Người giao hàng (Shipper)
    /// - Khách hàng (Customer)
    /// 
    /// Lớp này đóng vai trò trung gian giữa tầng giao diện (Presentation Layer)
    /// và tầng truy cập dữ liệu (Data Layer).
    /// </summary>
    public static class PartnerDataService
    {
        private static readonly IGenericRepository<Supplier> SupplierDB;
        private static readonly IGenericRepository<Shipper> ShipperDB;
        private static readonly ICustomerRepository CustomerDB;

        /// <summary>
        /// Hàm khởi tạo tĩnh của lớp.
        /// Khởi tạo các đối tượng truy cập dữ liệu tương ứng
        /// với từng loại đối tác trong hệ thống.
        /// </summary>
        static PartnerDataService()
        {
            SupplierDB = new SupplierRepository(Configuration.ConnectionString);
            ShipperDB = new ShipperRepository(Configuration.ConnectionString);
            CustomerDB = new CustomerRepository(Configuration.ConnectionString);
        }

        #region Supplier

        /// <summary>
        /// Tìm kiếm và trả về danh sách nhà cung cấp theo dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await SupplierDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin của một nhà cung cấp theo mã
        /// </summary>
        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            return await SupplierDB.GetAsync(supplierID);
        }

        /// <summary>
        /// Kiểm tra dữ liệu hợp lệ của nhà cung cấp
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns>
        /// Chuỗi thông báo lỗi. Nếu hợp lệ trả về chuỗi rỗng
        /// </returns>
        private static string ValidateSupplier(Supplier supplier)
        {
            if (supplier == null)
                return "Dữ liệu nhà cung cấp không hợp lệ";

            if (string.IsNullOrWhiteSpace(supplier.SupplierName))
                return "Tên nhà cung cấp không được để trống";

            if (string.IsNullOrWhiteSpace(supplier.ContactName))
                return "Tên người liên hệ không được để trống";

            if (string.IsNullOrWhiteSpace(supplier.Address))
                return "Địa chỉ không được để trống";

            if (string.IsNullOrWhiteSpace(supplier.Phone))
                return "Số điện thoại không được để trống";

            return "";
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới
        /// </summary>
        public static async Task<int> AddSupplierAsync(Supplier supplier)
        {
            //TODO: Kiểm tra tính hợp lệ của dữ liệu
            string error = ValidateSupplier(supplier);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await SupplierDB.AddAsync(supplier);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public static async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            //TODO: Kiểm tra tính hợp lệ của dữ liệu
            string error = ValidateSupplier(supplier);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await SupplierDB.UpdateAsync(supplier);
        }

        /// <summary>
        /// Xóa nhà cung cấp theo mã
        /// </summary>
        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await SupplierDB.IsUsedAsync(supplierID))
                return false;

            return await SupplierDB.DeleteAsync(supplierID);
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có dữ liệu liên quan không
        /// </summary>
        public static async Task<bool> IsUsedsupplierAsync(int supplierID)
        {
            return await SupplierDB.IsUsedAsync(supplierID);
        }

        #endregion


        #region Shipper

        /// <summary>
        /// Kiểm tra dữ liệu hợp lệ của người giao hàng
        /// </summary>
        /// <param name="shipper"></param>
        /// <returns>
        /// Chuỗi thông báo lỗi nếu dữ liệu không hợp lệ
        /// </returns>
        private static string ValidateShipper(Shipper shipper)
        {
            if (shipper == null)
                return "Dữ liệu người giao hàng không hợp lệ";

            if (string.IsNullOrWhiteSpace(shipper.ShipperName))
                return "Tên người giao hàng không được để trống";

            if (string.IsNullOrWhiteSpace(shipper.Phone))
                return "Số điện thoại không được để trống";

            return "";
        }

        /// <summary>
        /// Tìm kiếm danh sách người giao hàng theo dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await ShipperDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin của một người giao hàng theo mã
        /// </summary>
        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await ShipperDB.GetAsync(shipperID);
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới
        /// </summary>
        public static async Task<int> AddShipperAsync(Shipper shipper)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            string error = ValidateShipper(shipper);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await ShipperDB.AddAsync(shipper);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        public static async Task<bool> UpdateShipperAsync(Shipper shipper)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            string error = ValidateShipper(shipper);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await ShipperDB.UpdateAsync(shipper);
        }

        /// <summary>
        /// Xóa người giao hàng
        /// </summary>
        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            if (await ShipperDB.IsUsedAsync(shipperID))
                return false;

            return await ShipperDB.DeleteAsync(shipperID);
        }

        /// <summary>
        /// Kiểm tra người giao hàng có dữ liệu liên quan hay không
        /// </summary>
        public static async Task<bool> IsUsedShipperAsync(int shipperID)
        {
            return await ShipperDB.IsUsedAsync(shipperID);
        }

        #endregion


        #region Customer

        /// <summary>
        /// Kiểm tra dữ liệu hợp lệ của khách hàng
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>
        /// Chuỗi thông báo lỗi nếu dữ liệu không hợp lệ
        /// </returns>
        private static string ValidateCustomer(Customer customer)
        {
            if (customer == null)
                return "Dữ liệu khách hàng không hợp lệ";

            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                return "Tên khách hàng không được để trống";

            if (string.IsNullOrWhiteSpace(customer.ContactName))
                return "Tên người liên hệ không được để trống";

            if (string.IsNullOrWhiteSpace(customer.Address))
                return "Địa chỉ không được để trống";

            if (string.IsNullOrWhiteSpace(customer.Phone))
                return "Số điện thoại không được để trống";

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(customer.Email, pattern))
                    return "Email không hợp lệ";
            }

            return "";
        }

        /// <summary>
        /// Tìm kiếm danh sách khách hàng theo dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
        {
            return await CustomerDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của khách hàng theo mã
        /// </summary>
        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await CustomerDB.GetAsync(customerID);
        }

        /// <summary>
        /// Bổ sung khách hàng mới
        /// </summary>
        public static async Task<int> AddCustomerAsync(Customer customer)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            string error = ValidateCustomer(customer);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await CustomerDB.AddAsync(customer);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        public static async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            string error = ValidateCustomer(customer);
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            return await CustomerDB.UpdateAsync(customer);
        }

        /// <summary>
        /// Xóa khách hàng theo mã
        /// </summary>
        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            if (await CustomerDB.IsUsedAsync(customerID))
                return false;

            return await CustomerDB.DeleteAsync(customerID);
        }

        /// <summary>
        /// Kiểm tra khách hàng có dữ liệu liên quan trong hệ thống hay không
        /// </summary>
        public static async Task<bool> IsUsedCustomerAsync(int customerID)
        {
            return await CustomerDB.IsUsedAsync(customerID);
        }

        #endregion
    }
}