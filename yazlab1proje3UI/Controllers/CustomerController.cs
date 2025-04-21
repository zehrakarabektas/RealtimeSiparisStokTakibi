using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using yazlab1proje3UI.Classes;
using yazlab1proje3UI.Dtos.CustomerDtos;
using yazlab1proje3UI.Dtos.LoginDtos;
using yazlab1proje3UI.Dtos.ProductDtos;
using yazlab1proje3UI.Models;
using yazlab1proje3UI.Services;

namespace yazlab1proje3UI.Views
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;
        public static Random random = new Random();
        Hash sifrele = new Hash();

        public CustomerController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _httpClientFactory=httpClientFactory;
            _loginService=loginService;
        }
        [HttpGet]
        public IActionResult GetUserName()
        {
            var userName = _loginService.GetUserName; 
            return Ok(new { UserName = userName });
        }
        [HttpGet]
        public async Task<IActionResult> CustomerGirisi()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CustomerGirisi(CustomerLoginDto customerLoginDto)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(customerLoginDto),Encoding.UTF8,"application/json");
            var response = await clientWithHandler.PostAsync("https://localhost:44360/api/Login/CustomerLogin", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonData=await response.Content.ReadAsStringAsync();
                var tokenModel=System.Text.Json.JsonSerializer.Deserialize<JwtResponseModel>(jsonData,new JsonSerializerOptions
                {
                    PropertyNamingPolicy=JsonNamingPolicy.CamelCase
                });
                if (tokenModel != null)
                {
                    JwtSecurityTokenHandler handler=new JwtSecurityTokenHandler();
                    var token=handler.ReadJwtToken(tokenModel.Token);   
                    var claims=token.Claims.ToList();
                    if (tokenModel.Token!=null) 
                    {
                        claims.Add(new Claim("custtoken", tokenModel.Token));
                        var claimsIdendtity = new ClaimsIdentity(claims,JwtBearerDefaults.AuthenticationScheme);
                        var autgProps = new AuthenticationProperties
                        {
                            ExpiresUtc=tokenModel.ExpireDate,
                            IsPersistent=true
                        };
                        await HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdendtity),autgProps);
                        return RedirectToAction("CustomerAnaSayfa");
                    }

                }

            }
            return View();
        }
        public IActionResult Logout()
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie); 
            }

            return RedirectToAction("Index","Home");
        }


        [HttpPost]
        public async Task<IActionResult> CustomerKayit(CustomerSignUpDtos model)
        {
            if (ModelState.IsValid)
            {
                var yeniCustomer = new CreateCustomerDtos
                {
                    CustomerName = model.CustomerName,
                    Budget = random.Next(500, 2000),
                    CustomerType = "Standart",
                    TotalSpent =0,
                    CustomerEmail = model.CustomerMail,
                    Password = sifrele.Sifrele(model.Password),
                    Adress = model.Adress,
                    IsActive= true
                };
                var client = _httpClientFactory.CreateClient();
                var clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                var clientWithHandler = new HttpClient(clientHandler);

                var jsonData = JsonConvert.SerializeObject(yeniCustomer);

                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responceMessage = await clientWithHandler.PostAsync("https://localhost:44360/api/Customer", content);
               

                if (responceMessage.IsSuccessStatusCode)
                {
                    await Task.Delay(1000);
                    return RedirectToAction("CustomerGirisi");
                }
            }
            ViewBag.SignUpError = "Kayıt işlemi başarısız.";
            return View();
        }
        public async Task<IActionResult> Profilim()
        {
            var userId = _loginService.GetUserId;
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);
            var responseMessage = await clientWithHandler.GetAsync($"https://localhost:44360/api/Customer/{userId}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<ResultCustomerDtos>(jsonData);
                return View(customer);
            }
            else
            {
                ViewBag.ErrorMessage = "Müsteri bilgileri alınamadı.";
                return View();
            }
        }
      
        [Authorize]
        public async Task<IActionResult> CustomerAnaSayfa()
        {
            
            var userId = _loginService.GetUserId;
            var username=_loginService.GetUserName;
            var type = _loginService.GetCustType;
            var token=User.Claims.FirstOrDefault(x=>x.Type=="custtoken")?.Value;
            if (token != null)
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
            }
            return View();
        }
        public async Task<IActionResult> CustomerList()
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync("https://localhost:44360/api/Customer");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultCustomerDtos>>(jsonData);
                return View(values);
            }
            return View();
        }
       
       
    }
}
