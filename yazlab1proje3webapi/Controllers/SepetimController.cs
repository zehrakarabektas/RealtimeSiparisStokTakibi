using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Dtos.SepetimDtos;
using yazlab1proje3webapi.Repositories.SepetimRepositories;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SepetimController : ControllerBase
    {
        private readonly ISepetimRepository _sepetimRepository;

        public SepetimController(ISepetimRepository sepetimRepository)
        {
            _sepetimRepository=sepetimRepository;
        }

        [HttpPost("SepeteEkle")]
        public async Task<IActionResult> AddSepetim([FromBody] CreateSepetimDto urun)
        {
            bool result = await _sepetimRepository.AddSepetim(urun);
            if (result)
            {
                return Ok("Ürün sepete başarıyla eklendi veya güncellendi.");
            }
            return BadRequest("Ürün sepete eklenemedi.");
        }

        [HttpDelete("SepetiTemizle")]
        public async Task<IActionResult> AllDeleteSepetim(int customerId)
        {
            bool result = await _sepetimRepository.AllDeleteSepetim(customerId);
            if (result)
            {
                return Ok("Sepet başarıyla temizlendi.");
            }
            return NotFound("Silinecek sepet bulunamadı.");
        }

        [HttpDelete("UrunSil/{sepetimId}")]
        public async Task<IActionResult> DeleteSepetim(int sepetimId)
        {
            bool result = await _sepetimRepository.DeleteSepetim(sepetimId);
            if (result)
            {
                return Ok("Ürün sepetten başarıyla silindi.");
            }
            return NotFound("Silinecek ürün bulunamadı.");
        }

        [HttpGet("GetSepetim/{customerId}")]
        public async Task<IActionResult> GetCartByCustomerId(int customerId)
        {
            var sepetim = await _sepetimRepository.GetCartByCustomerId(customerId);
            if (sepetim ==null)
            {
                return NotFound("Müşterinin sepetinde ürün bulunamadı.");
            }
            return Ok(sepetim);
           
        }

        [HttpGet("SepetUrunSay/{customerId}")]
        public async Task<IActionResult> GetSepetimUrunSay(int customerId)
        {
            int totalQuantity = await _sepetimRepository.GetSepetimUrunSay(customerId);
            return Ok(new { TotalQuantity = totalQuantity });
        }
    }
}
