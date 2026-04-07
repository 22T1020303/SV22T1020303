using LiteCommerce.DataLayers.Interfaces;
using LiteCommerce.DataLayers.SQLServer;
using LiteCommerce.Models.Security;

namespace SV22T1020303.BusinessLayers
{
    public static class CustomerAccountDataService
    {
        private static readonly ICustomerAccountRepository _accountDB;

        static CustomerAccountDataService()
        {
            _accountDB = new CustomerAccountRepository(Configuration.ConnectionString);
        }

        public static async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _accountDB.ExistsByEmailAsync(email);
        }

        public static async Task<int> AddAsync(CustomerAccount account)
        {
            return await _accountDB.AddAsync(account);
        }

        public static async Task<CustomerAccount?> AuthorizeAsync(string email, string passwordHash)
        {
            return await _accountDB.AuthorizeAsync(email, passwordHash);
        }

        public static async Task<bool> ChangePasswordAsync(int customerId, string newPasswordHash)
        {
            return await _accountDB.ChangePasswordAsync(customerId, newPasswordHash);
        }

        public static async Task<CustomerAccount?> GetByCustomerIdAsync(int customerId)
        {
            return await _accountDB.GetByCustomerIdAsync(customerId);
        }
    }
}