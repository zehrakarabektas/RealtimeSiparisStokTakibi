using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Dtos.LogDtos;
using yazlab1proje3webapi.Dtos.OrderDtos;
using yazlab1proje3webapi.Repositories.LogRepositories;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ILogRepository _logRepository;

        public LogsController(ILogRepository logRepository)
        {
            _logRepository=logRepository;
        }
        [HttpGet]
        public async Task<IActionResult> LogListesi()
        {
            var loglar = await _logRepository.GetAllLog();

            if (loglar  == null ||  loglar.Count == 0)
            {
                return NotFound("Hiç log bulunamadı.");
            }

            return Ok(loglar);
        }
        [HttpPost]
        public async Task<IActionResult> LogEkle(CreateLogDto yeniLog)
        {
            _logRepository.AddLog(yeniLog);
            return Ok("Log başarıyla eklendi.");
        }
        [HttpDelete]
        public async Task<IActionResult> LogSil(int orderId)
        {
            _logRepository.DeleteLog(orderId);
            return Ok("Log başarıyla silindi.");
        }
        //[HttpGet("{orderId}")]
        //public async Task<IActionResult> LogGoruntuleme(int orderId)
        //{
        //    var log = await _logRepository.GetLog(orderId);
        //    if (log == null)
        //    {
        //        return NotFound("Sipariş bulunamadı.");
        //    }
        //    return Ok(log);
        //}
        [HttpPut]
        public async Task<IActionResult> LogGuncelle(UpdateLogDto log)
        {
            _logRepository.UpdateLog(log);
            return Ok("Log başarıyla güncellendi.");
        }
    }
}
