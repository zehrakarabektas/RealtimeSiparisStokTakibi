namespace yazlab1proje3webapi.Classes
{
    public class SemaphoreClass
    {
        public static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); 
        public static bool IsAdmin { get; set; } = false;
        public static bool IsAdminDelete { get; set; } = false;
    }
}
