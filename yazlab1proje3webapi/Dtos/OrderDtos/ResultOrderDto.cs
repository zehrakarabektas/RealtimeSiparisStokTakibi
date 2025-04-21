using System.ComponentModel.DataAnnotations;

namespace yazlab1proje3webapi.Dtos.OrderDtos
{
    public enum OrderStatusType
    {
        [Display(Name = "Onay Bekleyen")]
        OnayBeklemede = 0,
        [Display(Name = "İşlenen")]
        Isleniyor = 1,
        [Display(Name = "Tamamlanan")]
        Tamamlandi = 2,
        [Display(Name = "İptal Edilen")]
        IptalEdildi = 3,
        [Display(Name = "Beklemede Olan")]
        Beklemede = 4,
        [Display(Name = "Hata Alınan")]
        HataAlindi = 5,
        [Display(Name = "Bütçe Yetersizliği Hatası")]
        ButceYetersizligi = 6
    }

    public class ResultOrderDto
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
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
