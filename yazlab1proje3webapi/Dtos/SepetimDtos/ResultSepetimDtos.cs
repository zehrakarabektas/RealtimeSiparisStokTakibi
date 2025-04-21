namespace yazlab1proje3webapi.Dtos.SepetimDtos
{
    public class ResultSepetimDtos
    {
        public int Id { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; } 
        public string ProductName { get; set; } 
        public string ProductImage { get; set; } 
        public int Quantity { get; set; } 
        public decimal TotalPrice { get; set; } 
        public DateTime CreatedAt { get; set; } 
    }
}
