using yazlab1proje3UI.Dtos.CustomerDtos;

namespace yazlab1proje3UI.Classes
{
    public class RandomCustomer
    {
        public static Random random = new Random();
        int minCustomerSayisi = 5, maxCustomerSayisi = 10;
        int minPremiumCount = 2;
        int minBudget = 500, maxBudget = 3001;
        Hash sifrele = new Hash();
        public static HashSet<string> kullanilanAdlar = new HashSet<string>();

        public List<CreateCustomerDtos> RandomCustomerOlustur()
        {
            int customerSayisi = random.Next(minCustomerSayisi, maxCustomerSayisi + 1);
            List<CreateCustomerDtos> customers = new List<CreateCustomerDtos>();

            for (int i = 0; i < minPremiumCount; i++)
            {
                customers.Add(YeniPremiumCustomerEkle());
            }

            for (int i = minPremiumCount; i < customerSayisi; i++)
            {
                customers.Add(YeniCustomerEkle());
            }

            return customers;
        }

        private CreateCustomerDtos YeniPremiumCustomerEkle()
        {
            var randomname = RandomCustomerNameOlustur();
            var yeniCustomer = new CreateCustomerDtos
            {
                CustomerName = randomname,
                Budget = random.Next(minBudget, maxBudget),
                CustomerType = "Premium",
                TotalSpent = random.Next(2000, 4001),
                CustomerEmail = RandomEmailOlustur(randomname),
                Password = sifrele.Sifrele(RandomPasswordOlustur(randomname)),
                Adress = RandomAdressOlustur(),
                IsActive= true
            };
            return yeniCustomer;
        }

        private CreateCustomerDtos YeniCustomerEkle()
        {
            var randomname = RandomCustomerNameOlustur();
            var customerTypes ="Standart";
            var totalspent = random.Next(0, 2000);
            var yeniCustomer = new CreateCustomerDtos
            {
                CustomerName = randomname,
                Budget = random.Next(minBudget, maxBudget),
                CustomerType = "Standart",
                TotalSpent = totalspent,
                CustomerEmail = RandomEmailOlustur(randomname),
                Password = sifrele.Sifrele(RandomPasswordOlustur(randomname)),
                Adress = RandomAdressOlustur(),
                IsActive = true
            };
            return yeniCustomer;
        }

        private string RandomCustomerNameOlustur()
        {
            string[] ad = { "Alya", "Zehra", "Ela", "Ceylin", "Arda", "Selim", "Kerem", "Aren" };
            string[] soyad = { "Albayrak", "Kara", "Kurt", "Durak", "Turan", "Kasaba", "Sancar", "Mercan" };

            string customerad = ad[random.Next(ad.Length)];
            string customersoyad = soyad[random.Next(soyad.Length)];
            string customerName = $"{customerad} {customersoyad}";

            while (kullanilanAdlar.Contains(customerName))
            {
                customerad = ad[random.Next(ad.Length)];
                customersoyad = soyad[random.Next(soyad.Length)];
                customerName = $"{customerad} {customersoyad}";
            }

            kullanilanAdlar.Add(customerName);
            return $"{customerad} {customersoyad}";
        }

        private string RandomEmailOlustur(string randomname)
        {
            string customerNameWithoutSpaces = randomname.Replace(" ", "").ToLower();
            string mail = "gmail.com";
            return $"{customerNameWithoutSpaces}@{mail}";
        }

        private string RandomPasswordOlustur(string randomname)
        {
            string customerName = randomname.Replace(" ", "").ToLower();
            return $"{customerName}123";
        }

        private string RandomAdressOlustur()
        {
            string[] iller = {
                "Ankara", "Antalya", "Balıkesir", "Çanakkale", "İstanbul", "İzmir", "Kocaeli", "Nevşehir", "Rize", "Sakarya", "Trabzon"
            };

            string randomIl = iller[random.Next(iller.Length)];
            return $"{randomIl}";
        }
    }
}
