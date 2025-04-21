namespace yazlab1proje3UI.Dtos.SepetimDtos
{
    public class CreateSepetimDto
    {
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
