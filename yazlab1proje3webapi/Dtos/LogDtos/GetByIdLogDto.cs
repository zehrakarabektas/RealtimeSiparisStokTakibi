namespace yazlab1proje3webapi.Dtos.LogDtos
{
    public class GetByIdLogDto
    {
        public int LogID { get; set; }
        public int CustomerID { get; set; }
        public int OrderID { get; set; }
        public DateTime LogDate { get; set; }
        public string LogType { get; set; }
        public string LogDetails { get; set; }
    }
}
