namespace yazlab1proje3UI.Dtos.CustomerDtos
{
    public class UpdateCustomerDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Password { get; set; }
        public string Adress { get; set; }
        public decimal Budget { get; set; }
    }
}
