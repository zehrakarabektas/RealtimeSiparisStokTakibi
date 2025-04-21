namespace yazlab1proje3UI.Dtos.LogDtos
{
    public class ResultLogDto
    {
        public int LogID { get; set; }
        public int CustomerID { get; set; }
        public int? OrderID { get; set; }
        public DateTime LogDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string LogType { get; set; }
        public string CustomerType { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string LogDetails { get; set; }
    }
}
