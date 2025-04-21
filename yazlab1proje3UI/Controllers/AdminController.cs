using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using yazlab1proje3UI.Dtos.LoginDtos;
using yazlab1proje3UI.Models;
using yazlab1proje3UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace yazlab1proje3UI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;

        public AdminController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _httpClientFactory=httpClientFactory;
            _loginService=loginService;
        }
        [Authorize]
        public IActionResult Index()
        {
            var userName = _loginService.GetUserName;
            ViewData["UserName"] = userName;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AdminGirisi()
        {
            return View();
        }
        public IActionResult Logout()
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> AdminGirisi(AdminLoginDto adminLoginDto)
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var clientWithHandler = new HttpClient(clientHandler);
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(adminLoginDto), Encoding.UTF8, "application/json");
            var response = await clientWithHandler.PostAsync("https://localhost:44360/api/Login/AdminLogin", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var tokenModel = System.Text.Json.JsonSerializer.Deserialize<JwtResponseModel>(jsonData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy=JsonNamingPolicy.CamelCase
                });
                if (tokenModel != null)
                {
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(tokenModel.Token);
                    var claims = token.Claims.ToList();
                    if (tokenModel.Token!=null)
                    {
                        claims.Add(new Claim("admintoken", tokenModel.Token));
                        var claimsIdendtity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                        var autgProps = new AuthenticationProperties
                        {
                            ExpiresUtc=tokenModel.ExpireDate,
                            IsPersistent=true
                        };
                        await HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdendtity), autgProps);
                        return RedirectToAction("AdminAnaSayfa");
                    }

                }

            }
            return View();
        }
        [Authorize]
        public async Task<IActionResult>AdminAnaSayfa()
        {
            var userId = _loginService.GetUserId;
            var token = User.Claims.FirstOrDefault(x => x.Type=="admintoken")?.Value;
            if (token != null)
            {
                #region CustomerCount
                var client1 = _httpClientFactory.CreateClient();
                var clientHandler1 = new HttpClientHandler();
                clientHandler1.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                var clientWithHandler1 = new HttpClient(clientHandler1);
                var responseMessage1 = await clientWithHandler1.GetAsync("https://localhost:44360/api/Veri/CustomerCount");
                ViewBag.CustomerCount = int.Parse(await responseMessage1.Content.ReadAsStringAsync());
                #endregion

                #region  ProductCount
                var client2 = _httpClientFactory.CreateClient();
                var clientHandler2 = new HttpClientHandler();
                clientHandler2.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                var clientWithHandler2 = new HttpClient(clientHandler2);
                var responseMessage2 = await clientWithHandler2.GetAsync("https://localhost:44360/api/Veri/ProductCount");
                ViewBag.ProductCount = int.Parse(await responseMessage2.Content.ReadAsStringAsync());
                #endregion

                #region  OrderCount
                var client3 = _httpClientFactory.CreateClient();
                var clientHandler3 = new HttpClientHandler();
                clientHandler3.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                var clientWithHandler3 = new HttpClient(clientHandler3);
                var responseMessage3 = await clientWithHandler3.GetAsync("https://localhost:44360/api/Veri/OrderCount");
                ViewBag.OrderCount = int.Parse(await responseMessage3.Content.ReadAsStringAsync());
                #endregion

              
                return View();
            }

            return View();
        }
        //[HttpPut("OnaylaSiparisler")]
        //public async Task<IActionResult> OnaylaSiparisler()
        //{
        //    try
        //    {
        //        // Onay bekleyen siparişleri onayla
        //        await _orderService.SiparisleriOnayla();

        //        // Onaylı siparişleri işleme al
        //        await _orderService.OnayliOrderIslemleri();

        //        return Ok("Onay bekleyen siparişler başarıyla onaylandı ve işlendi.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
        //    }
        //}


    }
}
