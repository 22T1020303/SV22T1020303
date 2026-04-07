using LiteCommerce.Models.Security;

namespace LiteCommerce.DataLayers.Interfaces
{
    public interface ICustomerAccountRepository
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> AddAsync(CustomerAccount account);
        Task<CustomerAccount?> AuthorizeAsync(string email, string passwordHash);
        Task<bool> ChangePasswordAsync(int customerId, string newPasswordHash);
        Task<CustomerAccount?> GetByCustomerIdAsync(int customerId);
    }
}