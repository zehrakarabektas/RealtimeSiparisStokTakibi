using System.ComponentModel.DataAnnotations;

namespace yazlab1proje3UI.Dtos.LoginDtos
{
    public class CustomerSignUpDtos
    {
        public string CustomerName { get; set; }

        public string CustomerMail { get; set; }

        public string Password { get; set; }

        public string Adress { get; set; }
    }
}
