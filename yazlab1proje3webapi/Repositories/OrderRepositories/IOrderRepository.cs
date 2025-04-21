using yazlab1proje3webapi.Dtos;
using yazlab1proje3webapi.Dtos.OrderDtos;

namespace yazlab1proje3webapi.Repositories.OrderRepositories
{
    public interface IOrderRepository
    {
        Task<List<ResultOrderDto>> GetAllOrder();
        Task<List<ResultOrderWithCustomerDto>> GetOrdersByCustomerId(int customerId);
        Task<GetByOrderDto> GetOrder(int orderId);
        Task<int> AddOrder(CreateOrderDto order);
        Task<bool> UpdateOrder(UpdateOrderDto order);
        Task<bool> DeleteOrder(int orderId);
        Task UpdateOrderStatus(int orderId, OrderStatusType status);
        Task UpdateOncelikBeklemeSuresi(int orderId, double beklemeSuresi, double oncelikSkoru, OrderStatusType orderStatus);
        Task<List<ResultOrderDto>> GetOnayliSiparisler();
        //Task ProcessOrdersAsyncc(List<CreateOrderDto> newOrders, int productId);
        //void AddOrder(CreateOrderDto order);
        //void UpdateOrder(UpdateOrderDto order);
        //void DeleteOrder(int orderId);

    }
}
