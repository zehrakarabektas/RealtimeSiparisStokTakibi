using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using yazlab1proje3UI.Dtos.ProductDtos;
using yazlab1proje3UI.Services;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.ProductDtos;

namespace yazlab1proje3UI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;
        private SemaphoreClass _semaphoreClass;

        public ProductController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _httpClientFactory=httpClientFactory;
            _loginService=loginService;
        }

        public async Task<IActionResult> ProductList()
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; 

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync("https://localhost:44360/api/Products");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultProductDtos>>(jsonData);
                return View(values);
            }
            return View();
        }
        [HttpGet]
        public IActionResult ProductEkle()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProductEkle(CreateProductDtos yeniProduct)
        {
            if (yeniProduct.ProductImage != null && yeniProduct.ProductImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(yeniProduct.ProductImage.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await yeniProduct.ProductImage.CopyToAsync(stream);
                }

                yeniProduct.ImagePath = "/images/" + fileName;  
            }
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var jsonData=JsonConvert.SerializeObject(yeniProduct);

            StringContent content = new StringContent(jsonData,Encoding.UTF8,"application/json");
            var responceMessage = await clientWithHandler.PostAsync("https://localhost:44360/api/Products",content);
            if (responceMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }
            return View();
        }
        public async Task<IActionResult> ProductSil(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            clientWithHandler.DefaultRequestHeaders.Add("Admin", "true");
            SemaphoreClass.IsAdminDelete=true;
            var responseMessage = await clientWithHandler.DeleteAsync($"https://localhost:44360/api/Products/{id}");
            
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }
            return View();

        }
        [HttpGet]
        public async Task<IActionResult> ProductGuncelle(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            clientWithHandler.DefaultRequestHeaders.Add("Admin", "true");
            SemaphoreClass.IsAdmin=true;
            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Products/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<UpdateProductDtos>(jsonData);
                return View(product);
            }
            else
            {
                ViewBag.ErrorMessage = "Ürün bilgileri alınamadı.";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> ProductGuncelle(UpdateProductDtos guncelProduct)
        {
            if (guncelProduct.ProductImage != null && guncelProduct.ProductImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(guncelProduct.ProductImage.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await guncelProduct.ProductImage.CopyToAsync(stream);
                }

                guncelProduct.ImagePath = "/images/" + fileName;
            }
            else
            {
                var client1 = _httpClientFactory.CreateClient();
                var clientHandler1 = new HttpClientHandler();
                clientHandler1.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                var clientWithHandler1 = new HttpClient(clientHandler1);
                var responseMessage = await clientWithHandler1.GetAsync($"https://localhost:44360/api/Products/{guncelProduct.ProductID}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var jsonData1 = await responseMessage.Content.ReadAsStringAsync();
                    var product = JsonConvert.DeserializeObject<UpdateProductDtos>(jsonData1);
                    guncelProduct.ImagePath = product.ImagePath; // Mevcut görsel yolunu koru
                }
                else
                {
                    ViewBag.ErrorMessage = "Ürün bilgileri alınamadı.";
                    return View(guncelProduct);
                }
            }
            var yeniProduct = new ResultProductDtos
            {
                ProductID = guncelProduct.ProductID,
                Price = guncelProduct.Price,
                Stock=guncelProduct.Stock,
                ProductName = guncelProduct.ProductName,
                ImagePath = guncelProduct.ImagePath,
            };
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var jsonData = JsonConvert.SerializeObject(yeniProduct);

            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            clientWithHandler.DefaultRequestHeaders.Add("Admin", "true");
            SemaphoreClass.IsAdmin=true;
            var responceMessage = await clientWithHandler.PutAsync("https://localhost:44360/api/Products", content);
            if (responceMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }

            return View(guncelProduct);

        }


        [HttpGet]
        public async Task<IActionResult> ProductStockGuncelle(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            clientWithHandler.DefaultRequestHeaders.Add("Admin", "true");
            SemaphoreClass.IsAdmin=true;
            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Products/{id}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<UpdateProductDtos>(jsonData);
                return View(product);
            }
            else
            {
                ViewBag.ErrorMessage = "Ürün bilgileri alınamadı.";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> ProductStockGuncelle(UpdateProductDtos product)
        {
            var guncelProduct = new UpdateStockDto
            {
                ProductID = product.ProductID,
                Stock = product.Stock
            };

            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            var jsonData = JsonConvert.SerializeObject(guncelProduct);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            clientWithHandler.DefaultRequestHeaders.Add("Admin", "true");
            SemaphoreClass.IsAdmin=true;    
            var responseMessage = await clientWithHandler.PutAsync($"https://localhost:44360/api/Products/StockGuncelle", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }
            else
            {
                ViewBag.ErrorMessage = "Ürün bilgileri güncellenemedi.";
                return View();
            }

        }
       



    }
}
