using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.Customer;
using yazlab1proje3webapi.Models.Context;

namespace yazlab1proje3webapi.Repositories.CustomerRepositories
{
    public class CustomerRepository : ICustomerRepository
    {
        public readonly Context _context;
        public Hash sifrele=new Hash();
        private static readonly Mutex _mutex = new Mutex();

        public CustomerRepository(Context context)
        {
            _context=context;
        }

        public async void AddCustomer(CreateCustomerDto customer)
        {
            string query = "INSERT INTO Customers (CustomerName, CustomerEmail, Password, Adress, Budget, CustomerType, TotalSpent, IsActive)VALUES (@CustomerName, @CustomerEmail, @Password, @Adress, @Budget, @CustomerType, @TotalSpent, @IsActive)";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerName", customer.CustomerName);
            parameters.Add("@CustomerEmail", customer.CustomerEmail);
            parameters.Add("@Password", customer.Password);
            parameters.Add("@Adress", customer.Adress);
            parameters.Add("@Budget", customer.Budget);
            parameters.Add("@CustomerType", customer.CustomerType);
            parameters.Add("@TotalSpent", customer.TotalSpent);
            parameters.Add("@IsActive", customer.IsActive);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async void DeleteCustomer(int customerId)
        {
            string query = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

            using (var connection = _context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CustomerID", customerId);
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task<List<ResultCustomerDto>> GetAllCustomer()
        {
            string query = "SELECT CustomerID, CustomerName, CustomerEmail, Adress, Budget, CustomerType, TotalSpent, IsActive FROM Customers";

            using (var connection = _context.CreateConnection())
            {
                var customers=await connection.QueryAsync<ResultCustomerDto>(query);
                return customers.ToList();
            }
        }

        public async Task<GetByIdCustomerDto> GetByCustomer(int id)
        {
            string query = @" SELECT CustomerID, CustomerName, CustomerEmail, Adress, Budget, CustomerType, TotalSpent, IsActive 
                           FROM Customers WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", id);

            using (var connection = _context.CreateConnection())
            {
                var customer=await connection.QuerySingleOrDefaultAsync<GetByIdCustomerDto>(query, parameters);
                return customer;
            }
        }

        public async void UpdateCustomer(UpdateCustomerDto customer)
        {
            string query = @"UPDATE Customers SET
                      CustomerName = @CustomerName,
                      CustomerEmail = @CustomerEmail,
                      Adress = @Adress,
                      Budget = @Budget,
                      Password = @Password
                      WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerName", customer.CustomerName);
            parameters.Add("@CustomerEmail", customer.CustomerEmail);
            parameters.Add("@Adress", customer.Adress);
            parameters.Add("@Budget", customer.Budget);
            parameters.Add("@Password", sifrele.Sifrele(customer.Password));
            parameters.Add("@CustomerID", customer.CustomerID);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }
        public async Task UpdateCustomerBudgetAndTotalSpent(int customerId, decimal decBudget)
        {
            //if (decBudget <= 0)
            //{
            //    throw new ArgumentException("Harcama miktarı sıfırdan büyük olmalıdır.");
            //}

            string query = "UPDATE Customers SET Budget = Budget - @DecBudget, TotalSpent = TotalSpent + @DecBudget WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", customerId);
            parameters.Add("@DecBudget", decBudget);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, parameters);

                if (result == 0)
                {
                    throw new InvalidOperationException($"Müşteri ID: {customerId} için bütçe güncelleştirme işlemi başarısız. Geçersiz müşteri.");
                }
            }
        }
        public async Task UpdateCustomerType(int customerId, string customerType)
        {
            string query = "UPDATE Customers SET CustomerType = @CustomerType WHERE CustomerID = @CustomerID";
            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", customerId);
            parameters.Add("@CustomerType", customerType);
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, parameters);

                if (result == 0)
                {
                    throw new InvalidOperationException($"Müşteri ID: {customerId} için bütçe güncelleştirme işlemi başarısız. Geçersiz müşteri.");
                }
            }
        }

    }
}
