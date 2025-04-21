using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Models.Context;
using Dapper;
using System.Runtime.CompilerServices;

namespace yazlab1proje3webapi.Repositories.ProductRepositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly Context _context;

        public ProductRepository(Context context)
        {
            _context=context;
        }

        public async void AddProduct(CreateProductDto product) //bakcam status eklenebilir
        {
            string query = "INSERT INTO Products (ProductName, Stock, Price, ImagePath) VALUES (@ProductName, @Stock, @Price, @ImagePath)";

            var urun = new DynamicParameters();
            urun.Add("@ProductName", product.ProductName);
            urun.Add("@Stock", product.Stock);
            urun.Add("@Price", product.Price);
            urun.Add("@ImagePath", product.ImagePath);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, urun); 
            }
        }

        public async void DeleteProduct(int productId)
        {
            string updateOrdersQuery = "UPDATE Orders SET ProductID = NULL ,OrderStatus=3  WHERE ProductID = @ProductID and (OrderStatus = 0 or OrderStatus=4)";
            string updateOrdersQuery1 = "UPDATE Orders SET ProductID = NULL   WHERE ProductID = @ProductID ";
            
            string deleteSepetimQuery = "DELETE FROM Sepetim WHERE ProductID = @ProductID"; 
            string deleteProductQuery = "DELETE FROM Products WHERE ProductID = @ProductID";

            var urun = new DynamicParameters();
            urun.Add("@ProductID", productId);

            try
            {
                using (var connection = _context.CreateConnection())
                {

                    await connection.ExecuteAsync(updateOrdersQuery, urun);
                    await connection.ExecuteAsync(updateOrdersQuery1, urun);
                    await connection.ExecuteAsync(deleteSepetimQuery, urun);
                    await connection.ExecuteAsync(deleteProductQuery, urun);
                }


                Console.WriteLine($"ProductID {productId} başarıyla silindi ve ilişkili tablolardaki değerler NULL olarak güncellendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
            }
        }
        //string query = "DELETE FROM Products WHERE ProductID = @ProductID";

        //using (var connection = _context.CreateConnection())
        //{
        //    var urun = new DynamicParameters();
        //    urun.Add("@ProductID", productId);
        //    await connection.ExecuteAsync(query,urun);
        //}


        public async Task<List<ResultProductDto>> GetAllProduct()
        {
            string query = "Select * From Products";
            using (var connection = _context.CreateConnection()) 
            {
                var product = await connection.QueryAsync<ResultProductDto>(query);
                return product.ToList();
            }
        }

        public async Task<GetByIdProductDto> GetProduct(int productId)
        {
            string query = "SELECT * FROM Products WHERE ProductID = @ProductID";
            var urun = new DynamicParameters();
            urun.Add("@ProductID", productId);
            using (var connection = _context.CreateConnection())
            {
                var product= await connection.QueryFirstOrDefaultAsync<GetByIdProductDto>(query,urun);
                return product ;
            }
        }

        public async void UpdateProduct(UpdateProductDto product)
        {
            string query = "UPDATE Products SET ProductName = @ProductName, Stock = @Stock, Price = @Price, ImagePath = @ImagePath WHERE ProductID = @ProductID";

            var urun = new DynamicParameters();
            urun.Add("@ProductID", product.ProductID);
            urun.Add("@ProductName", product.ProductName);
            urun.Add("@Stock", product.Stock);
            urun.Add("@Price", product.Price);
            urun.Add("@ImagePath", product.ImagePath);
           
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, urun);  
            }
        }
        public async void UpdateProductStock(UpdateStockDto product)
        {
            string query = "UPDATE Products SET Stock = @Stock WHERE ProductID = @ProductID";

            var urun = new DynamicParameters();
            urun.Add("@ProductID", product.ProductID);
            urun.Add("@Stock", product.Stock);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, urun);
            }
        }
        public async Task<int> GetProductStock(int productId)
        {
            string query= "Select Stock From Products Where ProductID=@ProductID";

            var urun =new DynamicParameters();
            urun.Add("@ProductID", productId);
            using (var connection = _context.CreateConnection())
            {
                var stock = await connection.QueryFirstOrDefaultAsync<int>(query, urun);
                return stock;
            }
        }
    }
}
