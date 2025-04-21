using yazlab1proje3webapi.Dtos.Customer;

namespace yazlab1proje3webapi.Repositories.CustomerRepositories
{
    public interface ICustomerRepository
    {
        Task<List<ResultCustomerDto>> GetAllCustomer();
        Task<GetByIdCustomerDto> GetByCustomer(int id);
        void AddCustomer(CreateCustomerDto customer);
        void UpdateCustomer(UpdateCustomerDto customer);
        void DeleteCustomer(int customerId);
        Task UpdateCustomerBudgetAndTotalSpent(int customerId, decimal decBudget);
        Task UpdateCustomerType(int customerId, string customerType);
    }
}
