namespace yazlab1proje3UI.Dtos.CustomerDtos
{
    public class ResultCustomerDtos
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Adress { get; set; }
        public decimal Budget { get; set; }
        public string CustomerType { get; set; }
        public decimal TotalSpent { get; set; }
        public bool IsActive { get; set; }
    }
}
