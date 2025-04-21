namespace yazlab1proje3webapi.Dtos.LogDtos
{
    public class ResultLogWithCustomerDto
    {
        public int LogID { get; set; }
        public string CustomerName { get; set; }
        public int OrderID { get; set; }
        public DateTime LogDate { get; set; }
        public string LogType { get; set; }
        public string LogDetails { get; set; }
    }
}
