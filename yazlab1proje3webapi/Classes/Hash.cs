using System.Security.Cryptography;
using System.Text;

namespace yazlab1proje3webapi.Classes
{
    public class Hash
    {
        public string Sifrele(string sifre)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] baytlar = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(sifre));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in baytlar)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
        public bool SifreDogrula(string sifre, string hashlenmisSifre)
        {
            string sifreHashi = Sifrele(sifre);
            return sifreHashi.Equals(hashlenmisSifre);
        }
    }
}
