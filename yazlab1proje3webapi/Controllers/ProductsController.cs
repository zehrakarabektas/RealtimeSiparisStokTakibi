using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Repositories.ProductRepositories;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
        {
            _productRepository=productRepository;
            _logger=logger;
        }

        [HttpGet]
        public async Task<IActionResult> ProductListesi()
        {
            var urunler = await _productRepository.GetAllProduct();
            return Ok(urunler);
        }
        [HttpPost]
        public async Task<IActionResult> ProductEkle(CreateProductDto yeniProduct)
        {
            _productRepository.AddProduct(yeniProduct);
            return Ok("Ürün başarılı bir şekilde eklendi");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> ProductSil(int id)
        {
            var admin = Request.Headers["Admin"] == "true";
            if (admin)
            {
                _logger.LogInformation("Admin işlemi başladı. Semaphore bekleniyor...");
                SemaphoreClass.IsAdminDelete = true;
                await SemaphoreClass.semaphore.WaitAsync();
                _logger.LogInformation("Admin işlemi semaphore kilidini aldı.");
            }
            try
            {
                _productRepository.DeleteProduct(id);
                return Ok("Ürün başarıyla silindi.");
            }
            finally
            {
                if (admin)
                {
                    SemaphoreClass.IsAdminDelete = false;
                    SemaphoreClass.semaphore.Release();
                    _logger.LogInformation("Admin işlemi semaphore kilidini serbest bıraktı.");
                }
            }
           
        }
        [HttpPut]
        public async Task<IActionResult> ProductGuncelle(UpdateProductDto guncelProduct)
        {
            var admin = Request.Headers["Admin"] == "true";
            if (admin)
            {
                _logger.LogInformation("Admin işlemi başladı. Semaphore bekleniyor...");
                await SemaphoreClass.semaphore.WaitAsync();
                SemaphoreClass.IsAdmin=true;
                _logger.LogInformation("Admin işlemi semaphore kilidini aldı.");
              
            }
            try
            {
                _productRepository.UpdateProduct(guncelProduct);
                return Ok("Ürün başarıyla guncellendi.");
            }
            finally
            {
                if (admin)
                {
                    SemaphoreClass.semaphore.Release();
                    SemaphoreClass.IsAdmin=false;
                    _logger.LogInformation("Admin işlemi semaphore kilidini serbest bıraktı.");
                }
            }
            
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ProductGoruntuleme(int id)
        {
            var urun = await _productRepository.GetProduct(id);

            if (urun == null)
            {
                return NotFound($"Ürün ID {id} bulunamadı.");
            }

            return Ok(urun);
        }
        [HttpPut("StockGuncelle")]
        public async Task<IActionResult> ProductStockGuncelle(UpdateStockDto guncelProduct)
        {
            var admin = Request.Headers["Admin"] == "true";
            if (admin)
            {
                _logger.LogInformation("Admin işlemi başladı. Semaphore bekleniyor...");
                SemaphoreClass.IsAdmin = true;
                await SemaphoreClass.semaphore.WaitAsync();
                _logger.LogInformation("Admin işlemi semaphore kilidini aldı.");
            }
            try
            {
                _productRepository.UpdateProductStock(guncelProduct);
                return Ok("Ürün başarıyla guncellendi.");
            }
            finally
            {
                if (admin)
                {
                    SemaphoreClass.IsAdmin = false;
                    SemaphoreClass.semaphore.Release();
                    _logger.LogInformation("Admin işlemi semaphore kilidini serbest bıraktı.");
                }
            }
            
        }
        [HttpGet("GetStock/{productId}")]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            var stock = await _productRepository.GetProductStock(productId);

            if (stock < 0)
            {
                return NotFound($"Product ID {productId} için stok bilgisi bulunamadı.");
            }

            return Ok(stock);
        }




    }
}
