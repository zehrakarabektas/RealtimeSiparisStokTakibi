using Dapper;
using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Dtos.VeriDtos;
using yazlab1proje3webapi.Models.Context;

namespace yazlab1proje3webapi.Repositories.VeriRepositories
{
    public class VeriRepository:IVeriRepository
    {
        private readonly Context _context;

        public VeriRepository(Context context)
        {
            _context=context;
        }

        public int CustomerCount()
        {
            string query = "SELECT COUNT(*) FROM Customers;";

            using (var connection = _context.CreateConnection())
            {
                var result = connection.QueryFirstOrDefault<int>(query);
                return result;
            }
        }

        public async Task<List<ProductSalesDtos>> GetProductSales()
        {
            string query = @" SELECT o.ProductId, p.ProductName, SUM(o.Quantity) AS TotalSales FROM Orders o JOIN 
                            Products p ON o.ProductId = p.ProductId
                            GROUP BY  o.ProductId, p.ProductName ORDER BY  o.ProductId;";

            using (var connection = _context.CreateConnection())
            {
                var satisverisi = await connection.QueryAsync<ProductSalesDtos>(query);
                return satisverisi.ToList();
            }
        }
        public async Task<RevenueSummary> GetRevenueSummaryAsync()
        {
            string query = @"SELECT  SUM(CASE WHEN OrderStatus = 2 THEN TotalPrice ELSE 0 END) AS TotalRevenue,
                                     SUM(CASE WHEN OrderStatus = 3 THEN TotalPrice ELSE 0 END) AS LostRevenue
                                     FROM Orders;";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<RevenueSummary>(query);
                return result;
            }
        }

        public int OrderCount()
        {
            string query = "SELECT COUNT(*) FROM Orders;";

            using (var connection = _context.CreateConnection())
            {
                var result = connection.QueryFirstOrDefault<int>(query);
                return result;
            }
        }

        public int ProductCount()
        {
            string query = "SELECT COUNT(*) FROM Products;";

            using (var connection = _context.CreateConnection())
            {
                var result = connection.QueryFirstOrDefault<int>(query);
                return result;
            }
        }
        public async Task<List<CityCustomerCount>> GetCustomerCountsByCityAsync()
        {
            string query = @"SELECT Adress AS City,COUNT(*) AS CustomerCount FROM Customers GROUP BY  Adress ORDER BY CustomerCount DESC;";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<CityCustomerCount>(query);
                return result.ToList();
            }
        }

    }
}
