using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using yazlab1proje3UI.Dtos.LogDtos;
using yazlab1proje3UI.Services;

namespace yazlab1proje3UI.Controllers
{
    public class LogController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;

        public LogController(IHttpClientFactory httpClientFactory, ILoginService loginService)
        {
            _loginService=loginService;
            _httpClientFactory=httpClientFactory;
        }
        [HttpGet]
        public async Task<IActionResult> LogPaneli()
        {
            return View();
        }
       
        [HttpGet]
        public async Task<IActionResult> LogPaneliVeri()
        {
            var client = _httpClientFactory.CreateClient();
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var clientWithHandler = new HttpClient(clientHandler);

            var responseMessage = await clientWithHandler.GetAsync("https://localhost:44360/api/Logs");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultLogDto>>(jsonData);
                var sortedValues = values.OrderByDescending(log =>log.LogDate).ToList();

                return Ok(sortedValues);
            }
            return BadRequest("Siparişler alınırken bir hata oluştu.");
        }
    }
}
