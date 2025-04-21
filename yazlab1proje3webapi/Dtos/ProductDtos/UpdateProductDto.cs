namespace yazlab1proje3webapi.Dtos.ProductDtos
{
    public class UpdateProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }
}
