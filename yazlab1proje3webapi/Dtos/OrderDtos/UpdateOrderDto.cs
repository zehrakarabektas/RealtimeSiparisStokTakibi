namespace yazlab1proje3webapi.Dtos.OrderDtos
{
    public class UpdateOrderDto
    {
        public int OrderID { get; set; }
        //public int CustomerID { get; set; }
        //public int ProductID { get; set; }
        //public int Quantity { get; set; }
        //public decimal TotalPrice { get; set; }
        //public DateTime OrderDate { get; set; }
        public OrderStatusType OrderStatus { get; set; }
    }
}
