namespace yazlab1proje3UI.Dtos.OrderDtos
{
    public class ResultOrderWithCustomerDto
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatusType OrderStatus { get; set; }
        public double OncelikSkoru { get; set; }
        public double BeklemeSuresi { get; set; }
    }
}
