using Dapper;
using System.Data.Common;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.OrderDtos;
using yazlab1proje3webapi.Models.Context;

namespace yazlab1proje3webapi.Repositories.OrderRepositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly Context _context;

        public OrderRepository(Context context)
        {
            _context=context;
        }


        public async Task<int>  AddOrder(CreateOrderDto order)
        {
            string query = @"INSERT INTO Orders (CustomerID, ProductID, Quantity, TotalPrice, OrderDate, OrderStatus) OUTPUT INSERTED.OrderID VALUES (@CustomerID, @ProductID, @Quantity, @TotalPrice, @OrderDate, @OrderStatus)";

            var siparis = new DynamicParameters();
            siparis.Add("@CustomerID", order.CustomerID);
            siparis.Add("@ProductID", order.ProductID);
            siparis.Add("@Quantity", order.Quantity);
            siparis.Add("@TotalPrice", order.TotalPrice);
            siparis.Add("@OrderDate", order.OrderDate);
            siparis.Add("@OrderStatus", order.OrderStatus);
            using (var connection = _context.CreateConnection())
            {
                var orderId = await connection.ExecuteScalarAsync<int>(query, siparis);
                return orderId;
            }
        }

        public async Task<bool>  DeleteOrder(int orderId)
        {
            string query = "DELETE FROM Orders WHERE OrderID = @OrderID";

            using (var connection = _context.CreateConnection())
            {
                var siparis = new DynamicParameters();
                siparis.Add("@OrderID", orderId);
                await connection.ExecuteAsync(query, siparis);
                return true;
            }
        }

        public async Task<List<ResultOrderDto>> GetAllOrder()
        {
            string query = @"SELECT o.OrderID,o.CustomerID,c.CustomerName,c.CustomerType, o.ProductID,p.ProductName,p.ImagePath,o.Quantity,o.TotalPrice, o.OrderDate,o.OrderStatus,o.BeklemeSuresi,o.OncelikSkoru
                         FROM Orders o INNER JOIN Customers c ON o.CustomerID = c.CustomerID Left Join Products p ON o.ProductID = p.ProductID";

            using (var connection = _context.CreateConnection())
            {
                var siparisler = await connection.QueryAsync<ResultOrderDto>(query);
                return siparisler.ToList();
            }
        }

        public async Task<List<ResultOrderWithCustomerDto>> GetAllOrderWithCustomerProduct()
        {
            string query = "SELECT o.OrderID, c.CustomerName, p.ProductName,o.Quantity, o.TotalPrice, o.OrderDate, o.OrderStatus FROM Orders o INNER JOIN Customers c ON o.CustomerID = c.CustomerID INNER JOIN Products p ON o.ProductID = p.ProductID";

            using (var connection = _context.CreateConnection())
            {
                var siparisler = await connection.QueryAsync<ResultOrderWithCustomerDto>(query);
                return siparisler.ToList();
            }
        }

        public async Task<GetByOrderDto> GetOrder(int orderId)
        {
            string query = "SELECT * FROM Orders WHERE OrderID = @OrderID";
            var siparis = new DynamicParameters();
            siparis.Add("@OrderID", orderId);
            using (var connection = _context.CreateConnection())
            {
                var order = await connection.QueryFirstOrDefaultAsync<GetByOrderDto>(query, siparis);
                return order;
            }
        }

        public async Task<List<ResultOrderWithCustomerDto>> GetOrdersByCustomerId(int customerId)
        {
            string query = @"SELECT o.OrderID,  o.CustomerID AS CustomerID,  o.ProductID, c.CustomerName, 
            p.ProductName,   p.ImagePath, o.Quantity,  o.TotalPrice, o.OrderDate,  o.OrderStatus
            FROM  Orders o  INNER JOIN Customers c ON o.CustomerID = c.CustomerID  LEFT JOIN  Products p ON o.ProductID = p.ProductID  WHERE o.CustomerID = @CustomerID";

            using (var connection = _context.CreateConnection())
            {
                var orders = await connection.QueryAsync<ResultOrderWithCustomerDto>(query, new { CustomerID = customerId });

                return orders.ToList();
            }
        }


        public async Task<bool> UpdateOrder(UpdateOrderDto order)
        {
            string query = "UPDATE Orders SET OrderStatus = @OrderStatus WHERE OrderID = @OrderID";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, order);
                return true;
            }
        }
        public async Task UpdateOrderStatus(int orderId, OrderStatusType status)
        {
            string query = "UPDATE Orders SET OrderStatus = @OrderStatus WHERE OrderID = @OrderID";

            var parameters = new DynamicParameters();
            parameters.Add("@OrderID", orderId);
            parameters.Add("@OrderStatus", status);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }
        public async Task UpdateOncelikBeklemeSuresi(int orderId, double beklemeSuresi, double oncelikSkoru, OrderStatusType orderStatus)
        {
            var query = "UPDATE Orders SET OncelikSkoru = @OncelikSkoru, BeklemeSuresi = @BeklemeSuresi,OrderStatus = @OrderStatus WHERE OrderID = @OrderID";
            var parameters = new DynamicParameters();
            parameters.Add("@OrderID", orderId);
            parameters.Add("@OncelikSkoru", oncelikSkoru);
            parameters.Add("@BeklemeSuresi", beklemeSuresi);
            parameters.Add("@OrderStatus", orderStatus);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }
        public async Task<List<ResultOrderDto>> GetOnayliSiparisler()
        {
            string query = @"SELECT  o.OrderID, o.CustomerID,  c.CustomerName,  c.CustomerType, o.ProductID, p.ProductName, p.ImagePath, o.Quantity, o.TotalPrice, o.OrderDate,o.OrderStatus, o.BeklemeSuresi, o.OncelikSkoru
            FROM Orders o INNER JOIN Customers c ON o.CustomerID = c.CustomerID  INNER JOIN Products p ON o.ProductID = p.ProductID WHERE o.OrderStatus = @OrderStatus ORDER BY o.OncelikSkoru DESC";

            using (var connection = _context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OrderStatus", OrderStatusType.Beklemede);

                var orders = await connection.QueryAsync<ResultOrderDto>(query, parameters);

                return orders.ToList();
            }
        }



    }
}
