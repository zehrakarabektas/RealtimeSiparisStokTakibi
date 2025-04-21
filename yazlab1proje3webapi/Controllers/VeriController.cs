using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Repositories.VeriRepositories;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeriController : ControllerBase
    {
        private readonly IVeriRepository _veriRepository;

        public VeriController(IVeriRepository veriRepository)
        {
            _veriRepository=veriRepository;
        }
        [HttpGet("CustomerCount")]
        public IActionResult GetCustomerCount()
        {
            var count = _veriRepository.CustomerCount();
            return Ok(count);
        }
        [HttpGet("ProductCount")]
        public IActionResult GetProductCount()
        {
            var count = _veriRepository.ProductCount();
            return Ok(count);
        }

        [HttpGet("OrderCount")]
        public IActionResult GetOrderCount()
        {
            var count = _veriRepository.OrderCount();
            return Ok(count);
        }

        [HttpGet("ProductSales")]
        public async Task<IActionResult> GetProductSales()
        {
            var sales = await _veriRepository.GetProductSales();
            return Ok(sales);
        }

        [HttpGet("RevenueSummary")]
        public async Task<IActionResult> GetRevenueSummary()
        {
            var revenueSummary = await _veriRepository.GetRevenueSummaryAsync();
            return Ok(revenueSummary);
        }

        [HttpGet("CustomerCountsByCity")]
        public async Task<IActionResult> GetCustomerCountsByCity()
        {
            var customerCounts = await _veriRepository.GetCustomerCountsByCityAsync();
            return Ok(customerCounts);
        }
    }
}
