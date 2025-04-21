using Dapper;
using Microsoft.AspNetCore.DataProtection;
using yazlab1proje3webapi.Dtos.SepetimDtos;
using yazlab1proje3webapi.Models.Context;

namespace yazlab1proje3webapi.Repositories.SepetimRepositories
{
    public class SepetimRepository : ISepetimRepository
    {
        private readonly Context _context;

        public SepetimRepository(Context context)
        {
            _context=context;
        }

        public async Task<bool> AddSepetim(CreateSepetimDto urun)
        {
            string query = @"
    IF EXISTS (SELECT 1 FROM Sepetim WHERE CustomerID = @CustomerID AND ProductID = @ProductID)
    BEGIN
        UPDATE Sepetim
        SET Quantity = 
            CASE 
                WHEN Quantity + @Quantity > 5 THEN 5
                ELSE Quantity + @Quantity 
            END,
            TotalPrice = 
            CASE 
                WHEN Quantity + @Quantity > 5 THEN 5 * p.Price
                ELSE TotalPrice + (@Quantity * p.Price) 
            END
        FROM Sepetim s
        INNER JOIN Products p ON s.ProductID = p.ProductID
        WHERE s.CustomerID = @CustomerID AND s.ProductID = @ProductID;
    END
    ELSE
    BEGIN
        INSERT INTO Sepetim (CustomerID, ProductID, Quantity, TotalPrice, CreatedAt)
        SELECT 
            @CustomerID, 
            @ProductID, 
            CASE 
                WHEN @Quantity > 5 THEN 5 
                ELSE @Quantity 
            END, 
            CASE 
                WHEN @Quantity > 5 THEN 5 * p.Price
                ELSE @Quantity * p.Price 
            END, 
            @CreatedAt
        FROM Products p
        WHERE p.ProductID = @ProductID;
    END";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", urun.CustomerID);
            parameters.Add("@ProductID", urun.ProductID);
            parameters.Add("@Quantity", urun.Quantity);
            parameters.Add("@CreatedAt", urun.CreatedAt);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
                return true;
            }
        }


        public async Task<bool> AllDeleteSepetim(int customerId)
        {
            string query = "DELETE FROM Sepetim WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", customerId);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
                return true;
            }
        }

        public async Task<bool> DeleteSepetim(int sepetimId)
        {
            string query = "DELETE FROM Sepetim WHERE Id = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", sepetimId);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
                return true;
            }
        }

        public async Task<IEnumerable<ResultSepetimDtos>> GetCartByCustomerId(int customerId)
        {
            string query = @" SELECT  s.Id,  s.CustomerID, s.ProductID,  p.ProductName AS ProductName, p.ImagePath AS ProductImage,s.Quantity, s.TotalPrice, s.CreatedAt
                          FROM Sepetim s  INNER JOIN Products p ON s.ProductID = p.ProductID  WHERE s.CustomerID = @CustomerID";

            using (var connection = _context.CreateConnection())
            {
                var sepetim = await connection.QueryAsync<ResultSepetimDtos>(query, new { CustomerID = customerId });

                return sepetim.ToList();
               
            }
        }
        public async Task<int> GetSepetimUrunSay(int customerId)
        {
            //COALESCE(SUM(Quantity), 0) yoksa 0 dondurmesi için
            string query = "SELECT COALESCE(SUM(Quantity), 0) FROM Sepetim WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", customerId);

            using (var connection = _context.CreateConnection())
            {
                int totalQuantity = await connection.ExecuteScalarAsync<int>(query, parameters);
                return totalQuantity;
            }
        }
    }
}
