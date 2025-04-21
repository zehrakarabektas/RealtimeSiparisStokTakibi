using System.ComponentModel.DataAnnotations.Schema;

namespace yazlab1proje3UI.Dtos.ProductDtos
{
    public class UpdateProductDtos
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public IFormFile ProductImage { get; set; }
    }
}
