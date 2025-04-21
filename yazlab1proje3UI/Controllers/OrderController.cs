using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using yazlab1proje3UI.Dtos.LogDtos;
using yazlab1proje3UI.Dtos.OrderDtos;
using yazlab1proje3UI.Dtos.SepetimDtos;
using yazlab1proje3UI.Services;
using yazlab1proje3webapi.Classes;
using OrderStatusType = yazlab1proje3UI.Dtos.OrderDtos.OrderStatusType;
using ResultOrderWithCustomerDto = yazlab1proje3UI.Dtos.OrderDtos.ResultOrderWithCustomerDto;
using UpdateOncelikSkorBeklemeSureDtos = yazlab1proje3UI.Dtos.OrderDtos.UpdateOncelikSkorBeklemeSureDtos;
using UpdateOrderDto = yazlab1proje3UI.Dtos.OrderDtos.UpdateOrderDto;

namespace yazlab1proje3UI.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;
        private static volatile bool _isAdminOrderComplete = false;
        public OrderController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _loginService=loginService;
            _httpClientFactory=httpClientFactory;
        }


        public IActionResult Index()
        {
            return View();
        }
      
        [HttpGet]
        public async Task<IActionResult> AdminOrderList()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AdminOrderListVeri()
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync("https://localhost:44360/api/Orders");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultOrderDtos>>(jsonData);
                var sortedValues = values.OrderByDescending(order => order.OncelikSkoru).ToList();

                return Ok(sortedValues);
            }
            return BadRequest("Siparişler alınırken bir hata oluştu.");
        }
      
        [HttpPost]
        public IActionResult AdminOrderOnaylama()
        {
          
            var butonaBasildi = DateTime.Now;
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);


            Thread backgroundThread = new Thread(async () =>
            {
                var response = await clientWithHandler.GetAsync("https://localhost:44360/api/Orders");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Siparişler alınırken hata oluştu.");
                    return;
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var siparisler = JsonConvert.DeserializeObject<List<ResultOrderDtos>>(jsonData);
               
                var onayBeklemeSiparisleri = siparisler
                    .Where(order => order.OrderStatus == OrderStatusType.OnayBeklemede || order.OncelikSkoru == 0||order.OrderStatus == OrderStatusType.Beklemede)
                    .OrderByDescending(order => order.OncelikSkoru)
                    .ToList();
                var countdownEvent = new CountdownEvent(onayBeklemeSiparisleri.Count);

                foreach (var order in onayBeklemeSiparisleri)
                {
                    Thread siparisThread = new Thread(async () =>
                    {
                        try
                        {
                            var random = new Random();
                            int delay = random.Next(500, 10000);
                            Thread.Sleep(delay);
                            double beklemeSuresi = (butonaBasildi - order.OrderDate).TotalSeconds;
                            var temelOncelikSkoru = order.CustomerType == "Premium" ? 15 : 10;
                            var oncelikSkoru = temelOncelikSkoru + (beklemeSuresi * 0.5);

                            var updateOrder = new UpdateOncelikSkorBeklemeSureDtos
                            {
                                BeklemeSuresi = beklemeSuresi,
                                OncelikSkoru = oncelikSkoru,
                                OrderStatus = OrderStatusType.Beklemede
                            };

                            var jsonContent = new StringContent(JsonConvert.SerializeObject(updateOrder), Encoding.UTF8, "application/json");

                            var updateResponse = await clientWithHandler.PutAsync($"https://localhost:44360/api/Orders/UpdateOncelikBeklemeSuresi/{order.OrderID}", jsonContent);

                            if (updateResponse.IsSuccessStatusCode)
                            {
                                string logMessage = $"Admin, {order.CustomerName} adlı müşterinin {order.OrderID} numaralı siparişini onayladı.";
                                await LogOlustur(order.CustomerID, order.OrderID, "Başarılı", logMessage);
                                Console.WriteLine($"Sipariş ID {order.OrderID} başarıyla güncellendi.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Sipariş ID {order.OrderID} işlenirken hata: {ex.Message}");
                        }
                        finally
                        {
                            countdownEvent.Signal(); 
                        }
                       
                    });

                    siparisThread.IsBackground = true;
                    siparisThread.Start();
                }
                countdownEvent.Wait();
                countdownEvent.Dispose();
                var siparisIslemeResponse = await clientWithHandler.PostAsync("https://localhost:44360/api/Orders/SiparisIsleme", null);

                if (siparisIslemeResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Sipariş işleme işlemi başarıyla başlatıldı.");
                }
                else
                {
                    Console.WriteLine($"Sipariş işleme işlemi başlatılırken hata oluştu: {siparisIslemeResponse.ReasonPhrase}");
                }
            });

            backgroundThread.IsBackground = true;
            backgroundThread.Start();

            return NoContent();
        }
       
        public async Task<IActionResult> CustomerOrderList()
        {
            var userId = _loginService.GetUserId;
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Orders/Customer/{userId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultOrderWithCustomerDto>>(jsonData);
                var sortedValues = values.OrderByDescending(order => order.OrderDate).ToList();

                return View(sortedValues);
            }
            return View();
        }
        [HttpPost("/CreateOrder")]
        public async Task<IActionResult> CreateOrder()
        {
            var butonBasti = DateTime.Now;         
            var customerId = int.Parse(_loginService.GetUserId);
            var username = _loginService.GetUserName;
            var type = _loginService.GetCustType;
            var maxTime = 15;
           
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Sepetim/GetSepetim/{customerId}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorMessage = await responseMessage.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Sepet verileri alınamadı: {errorMessage}";
                return RedirectToAction("CustomerOrderList");
            }

            var responseData = await responseMessage.Content.ReadAsStringAsync();
            var sepettekiler = JsonConvert.DeserializeObject<List<ResultSepetimDtos>>(responseData);

            if (sepettekiler == null || !sepettekiler.Any())
            {
                TempData["ErrorMessage"] = "Sepetinizde ürün bulunamadı.";
                return RedirectToAction("CustomerOrderList");
            }
            var toplamSepetTutari = sepettekiler.Sum(x => x.TotalPrice);
            decimal customerBudget;
            try
            {
                customerBudget = await GetCustomerBudgetAsync(customerId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("CustomerOrderList");
            }
          
            foreach (var sepetUrun in sepettekiler)
            {
                var logMessage = new StringBuilder();
                logMessage.Append($"Müşteri {username} ({type})");
                var order = new CreateOrderDtos
                {
                    CustomerID = customerId,
                    ProductID = sepetUrun.ProductID,
                    Quantity = sepetUrun.Quantity,
                    TotalPrice = sepetUrun.TotalPrice,
                    OrderDate = DateTime.Now,
                    OrderStatus = OrderStatusType.OnayBeklemede,
                };

                var createOrderContent = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                var createOrderResponse = await clientWithHandler.PostAsync("https://localhost:44360/api/Orders", createOrderContent);

                var responseContent = await createOrderResponse.Content.ReadAsStringAsync();
                var orderResponse = JsonConvert.DeserializeObject<OrderIdGet>(responseContent);
                var orderId = orderResponse.OrderID;
                logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity} adet sipariş verdi.");
                if (SemaphoreClass.IsAdmin == true)     //surayı sonradan ekledim dene
                {
                    logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity}  adet siparişi sırasında  admin stok güncelleme işlemi algılandı.İşlemin bitmesi bekleniyor...");
                    await LogOlustur(customerId, orderId, "Başarılı", logMessage.ToString());
                    while (SemaphoreClass.IsAdmin == true)
                    {
                        Thread.Sleep(500);
                    }
                    logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity} adet siparişine devam ediliyor...Admin stok güncelleme işlemi tamamlandı.");
                    await LogOlustur(customerId, orderId, "Bilgilendirme", logMessage.ToString());
                  
                }
                if (SemaphoreClass.IsAdminDelete == true)
                {
                    logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity} adet siparişi sırasında  admin ürün silme işlemi algılandı.İşlemin bitmesi bekleniyor...");
                    await LogOlustur(customerId, orderId, "Bilgilendirme", logMessage.ToString());

                    while (SemaphoreClass.IsAdminDelete == true)
                    {
                        Thread.Sleep(500);
                    }
                    logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity} adet siparişine devam ediliyor...İşlemin bitmesi bekleniyor...Admin ürün silme işlemi tamamlandı");
                    await LogOlustur(customerId, orderId, "Bilgilendirme", logMessage.ToString());

                }
                var product = await clientWithHandler.GetAsync($"https://localhost:44360/api/Products/{order.ProductID}");
                if (product == null)
                {
                    logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity}  adet siparişi ürünün silinmesi nedeniyle iptal edildi...");
                    var orderguncel = new UpdateOrderDto
                    {
                        OrderID=orderId,
                        OrderStatus = OrderStatusType.IptalEdildi,
                    };
                    var client3 = _httpClientFactory.CreateClient();
                    var clientHandler3 = new HttpClientHandler();
                    clientHandler3.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                    var clientWithHandler3 = new HttpClient(clientHandler3);
                    var content = new StringContent(JsonConvert.SerializeObject(orderguncel), Encoding.UTF8, "application/json");
                    var response = await clientWithHandler3.PutAsync("https://localhost:44360/api/Orders", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Zaman aşımı güncelleme hatası: {errorMessage}");
                    }
                    continue;
                }   //suraya kadar kontrol et
                if (!createOrderResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await createOrderResponse.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Sipariş oluşturulamadı: {errorMessage}";
                    continue;
                }
                logMessage.AppendLine($"{sepetUrun.ProductName}’den {sepetUrun.Quantity} adet sipariş verdi.");
                await LogOlustur(customerId, orderId, "Başarılı", logMessage.ToString());
                var gecenTime = (butonBasti - sepetUrun.CreatedAt).TotalSeconds;
         
                if (gecenTime > maxTime) 
                {
                    await TimeoutError( customerId,  orderId,sepetUrun.ProductName,sepetUrun.Quantity);
                    TempData["ErrorMessage"] = $"{sepetUrun.ProductName} için işlem zaman aşımına uğradı.";
                }
                else if (toplamSepetTutari > customerBudget)
                {
                    await BudgetError(orderId,toplamSepetTutari, sepetUrun.ProductName,customerBudget);
                    TempData["ErrorMessage"] = $"Bütçe yetersizliği nedeniyle tüm siparişler iptal edildi. Toplam Tutar: {toplamSepetTutari:C}, Bütçe: {customerBudget:C}";
                }

                var sepetDeleteResponse = await clientWithHandler.DeleteAsync($"https://localhost:44360/api/Sepetim/UrunSil/{sepetUrun.Id}");
                if (!sepetDeleteResponse.IsSuccessStatusCode)
                {
                    var deleteErrorMessage = await sepetDeleteResponse.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Sipariş oluşturuldu ancak ürün sepetten silinemedi: {deleteErrorMessage}";
                    return RedirectToAction("CustomerOrderList");
                }
            }
           
            TempData["SuccessMessage"] = "Sipariş başarıyla oluşturuldu.";
            return RedirectToAction("CustomerOrderList");
        }


        private async Task LogOlustur(int customerId, int orderId,string logType, string logDetails)
        {
           var log=new CreateLogDto
            {
                CustomerID = customerId,
                LogType = logType,
                OrderID = orderId,
                LogDate = DateTime.Now,
                LogDetails= logDetails
            };
            var client1 = _httpClientFactory.CreateClient();
            var clientHandler1 = new HttpClientHandler();
            clientHandler1.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler1 = new HttpClient(clientHandler1);
            var createLogContent = new StringContent(JsonConvert.SerializeObject(log), Encoding.UTF8, "application/json");
            await clientWithHandler1.PostAsync($"https://localhost:44360/api/Logs",createLogContent);

        }
        private async Task BudgetError(int orderId, decimal toplamSepetTutari,string productName,decimal customerBudget)
        {
            var customerId = int.Parse(_loginService.GetUserId);
            var username = _loginService.GetUserName;
            var type = _loginService.GetCustType;

            try
            {
                var logMessage = $"Müşteri {username} ({type}) siparişi bütçe yetersizliğinden iptal edildi.Fiyat: {toplamSepetTutari:C}, Bütçe: {customerBudget:C}";
                Console.WriteLine($"Log Message Length: {logMessage.Length}");
                Console.WriteLine(logMessage);

                await LogOlustur(customerId, orderId, "Hata", logMessage);
                var orderguncel= new UpdateOrderDto
                {
                    OrderID=orderId,
                    OrderStatus = OrderStatusType.ButceYetersizligi, 
                };

                var client3 = _httpClientFactory.CreateClient();
                var clientHandler3 = new HttpClientHandler();
                clientHandler3.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                var clientWithHandler3 = new HttpClient(clientHandler3);
                var content = new StringContent(JsonConvert.SerializeObject(orderguncel), Encoding.UTF8, "application/json");
                var response = await clientWithHandler3.PutAsync("https://localhost:44360/api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Zaman aşımı güncelleme hatası: {errorMessage}");
                }

            }
            catch (Exception ex)
            {
                await LogOlustur(customerId, 0, "Hata", $"Veritabanı hatası: {ex.Message}");
            }
        }
        private async Task TimeoutError(int customerId, int orderId, string productName, int quantity)
        {
            var logMessage = $"Müşteri {customerId}, {productName}’den {quantity} adet sipariş vermek istedi ancak işlem süreci zaman aşımına uğradı.";
            await LogOlustur(customerId, orderId, "Hata", logMessage);

            var orderguncel = new UpdateOrderDto
            {
                OrderID = orderId,
                OrderStatus = OrderStatusType.HataAlindi
            };

            var client3 = _httpClientFactory.CreateClient();
            var clientHandler3 = new HttpClientHandler();
            clientHandler3.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler3 = new HttpClient(clientHandler3);
            var content = new StringContent(JsonConvert.SerializeObject(orderguncel), Encoding.UTF8, "application/json");
            var response = await clientWithHandler3.PutAsync("https://localhost:44360/api/Orders", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Zaman aşımı güncelleme hatası: {errorMessage}");
            }
        }
        public async Task<decimal> GetCustomerBudgetAsync(int customerId)
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);
            var response = await clientWithHandler.GetAsync($"https://localhost:44360/api/Customer/{customerId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Müşteri bütçesi alınamadı: {errorMessage}");
            }

            var responseData = await response.Content.ReadAsStringAsync();

            var budget = JObject.Parse(responseData)["budget"]?.Value<decimal>();

            if (budget == null)
            {
                throw new Exception("Müşteri bütçe bilgisi bulunamadı.");
            }

            return budget.Value;
        }

        public async Task<IActionResult> CustomerOrderIptali(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.DeleteAsync($"https://localhost:44360/api/Orders/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var message = await responseMessage.Content.ReadAsStringAsync();
                return RedirectToAction("CustomerOrderList");
            }
            return View();
        }

    }
}
