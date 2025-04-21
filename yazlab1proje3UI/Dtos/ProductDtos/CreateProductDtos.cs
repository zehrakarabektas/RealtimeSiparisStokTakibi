namespace yazlab1proje3UI.Dtos.ProductDtos
{
    public class CreateProductDtos
    {
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public IFormFile ProductImage { get; set; }
        public string ImagePath { get; set; }
    }
}
