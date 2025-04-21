using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Security;
using System.Net;
using yazlab1proje3UI.Dtos.ProductDtos;
using yazlab1proje3UI.Models;
using System.Security.Cryptography.X509Certificates;
using yazlab1proje3UI.Classes;
using yazlab1proje3UI.Dtos.CustomerDtos;
using System.Text;
using yazlab1proje3UI.Services;
using System;

namespace yazlab1proje3UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        
        //HashModeli sifrele = new HashModeli();

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory) 
        {
            _logger = logger;
            _httpClientFactory=httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            
            var client = _httpClientFactory.CreateClient();

            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; 

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync("https://localhost:44360/api/Products");

            List<ResultProductDtos> values = null;
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                values = JsonConvert.DeserializeObject<List<ResultProductDtos>>(jsonData);
            }
            RandomCustomer randomCustomerOlustur = new RandomCustomer();
            List<CreateCustomerDtos> customers = randomCustomerOlustur.RandomCustomerOlustur();

            foreach (var customer in customers)
            {
                var customerJson = JsonConvert.SerializeObject(customer);
                StringContent content = new StringContent(customerJson, Encoding.UTF8, "application/json");

                var response = await clientWithHandler.PostAsync("https://localhost:44360/api/Customer", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Müþteri baþarýyla eklendi: {customer.CustomerName}");
                }
                else
                {
                    Console.WriteLine($"Hata oluþtu: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Hata içeriði: {errorContent}");
                }

                await Task.Delay(1000);
            }
            return View(values);
        }

       


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
     



    }
}
