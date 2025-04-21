using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using yazlab1proje3UI.Controllers;
using yazlab1proje3UI.Dtos.SepetimDtos;
using yazlab1proje3UI.Services;

namespace yazlab1proje3UI.Controllers
{
    [Route("Sepetim")]
    public class SepetimController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;

        public SepetimController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _httpClientFactory=httpClientFactory;
            _loginService=loginService;
        }
        [HttpPost("AddSepet")]
        public async Task<IActionResult> AddSepet([FromBody] CreateSepetDto product)
        {
            var customerId = _loginService.GetUserId;
            var sepetEklencekProduct = new CreateSepetimDto
            {
                CustomerID=int.Parse(customerId),
                ProductID=product.ProductID,
                Quantity=product.Quantity,
                CreatedAt=DateTime.Now
            };
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);
            var jsonData = JsonConvert.SerializeObject(sepetEklencekProduct);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responceMessage = await clientWithHandler.PostAsync("https://localhost:44360/api/Sepetim/SepeteEkle", content);
            if (responceMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("CustomerAnaSayfa", "Customer");
            }
            else
            {
                ViewBag.ErrorMessage = "Ürün sepete eklenemedi.";
                return View("CustomerAnaSayfa", "Customer");
            }

        }
        [HttpDelete("AllSepetProductSil")]
        public async Task<IActionResult> AllDeleteSepetim()
        {
            var customerId = int.Parse(_loginService.GetUserId);

            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            var responseMessage = await clientWithHandler.DeleteAsync($"https://localhost:44360/api/Sepetim/SepetiTemizle/{customerId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                return Ok("Sepet başarıyla temizlendi.");
            }

            return NotFound("Sepette silinecek ürün bulunamadı.");
        }
        [HttpGet("GetCartByCustomerId")]
        public async Task<IActionResult> GetCartByCustomerId()
        {
            var customerId = int.Parse(_loginService.GetUserId);
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Sepetim/GetSepetim/{customerId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultSepetimDtos>>(jsonData);
                return PartialView("_GetCartByCustomerId", values);
            }
            return View();

        }
       
        [HttpPost("DeleteSepetim")]
        public async Task<IActionResult> DeleteSepetim(int sepetimId)
        {
            var customerId = int.Parse(_loginService.GetUserId);
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            var responseMessage = await clientWithHandler.DeleteAsync($"https://localhost:44360/api/Sepetim/UrunSil/{sepetimId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("GetSepetim");
            }
        
            return View();
        }

        [HttpGet("GetSepetUrunSayisi")]
        public async Task<IActionResult> GetSepetimUrunSay()
        {
            var customerId = int.Parse(_loginService.GetUserId);
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);
            var responceMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Sepetim/SepetUrunSay/{customerId}");

            if (responceMessage.IsSuccessStatusCode)
            {
                var json = await responceMessage.Content.ReadAsStringAsync();

                var toplamQuantityProduct = JsonConvert.DeserializeObject<dynamic>(json);

                return Ok(toplamQuantityProduct);
            }

            return NotFound("Sepet toplam miktarı alınamadı.");
        }
        [HttpGet("GetSepetim")]
        public async Task<IActionResult> GetSepetim()
        {
            var customerId = int.Parse(_loginService.GetUserId);
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Sepetim/GetSepetim/{customerId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultSepetimDtos>>(jsonData);
                return View(values);
            }
            return View();

        }

    }
}


