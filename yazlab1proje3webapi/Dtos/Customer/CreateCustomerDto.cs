namespace yazlab1proje3webapi.Dtos.Customer
{
    public class CreateCustomerDto
    {
        public string CustomerName { get; set; }     
        public string CustomerEmail { get; set; }   
        public string Password { get; set; }          
        public string Adress { get; set; }          
        public decimal Budget { get; set; }           
        public string CustomerType { get; set; }       
        public decimal TotalSpent { get; set; }
        public bool IsActive { get; set; }
    }
}
