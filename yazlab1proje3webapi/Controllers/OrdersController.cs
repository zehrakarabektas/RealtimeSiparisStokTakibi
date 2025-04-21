using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.LogDtos;
using yazlab1proje3webapi.Dtos.OrderDtos;
using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Repositories.CustomerRepositories;
using yazlab1proje3webapi.Repositories.LogRepositories;
using yazlab1proje3webapi.Repositories.OrderRepositories;
using yazlab1proje3webapi.Repositories.ProductRepositories;
using yazlab1proje3webapi.Services;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderServices _order;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogRepository _logRepository;
        private static readonly ConcurrentDictionary<int, Mutex> _productMutex = new();
        private static readonly Mutex _updateMutex = new();
        public OrdersController(IOrderRepository orderRepository, IOrderServices order, IProductRepository productRepository, ICustomerRepository customerRepository, ILogRepository logRepository)
        {
            _orderRepository=orderRepository;
            _order=order;
            _productRepository=productRepository;
            _customerRepository=customerRepository;
            _logRepository=logRepository;
        }

        [HttpGet]
        public async Task<IActionResult> OrderListesi()
        {
            var siparis = await _orderRepository.GetAllOrder();

            if (siparis  == null || siparis.Count == 0)
            {
                return NotFound("Hiç sipariş bulunamadı.");
            }

            return Ok(siparis);
        }

        [HttpPost]
        public async Task<IActionResult> OrderEkle( CreateOrderDto yeniorder)
        {
            var orderId = await _orderRepository.AddOrder(yeniorder);
            return Ok(new { OrderID = orderId });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> OrderSil(int id)
        {
            //_orderRepository.DeleteOrder(id);
            //return Ok("Sipariş başarıyla silindi.");
            var order = await _orderRepository.GetOrder(id);
            if (order == null)
            {
                return NotFound("Sipariş bulunamadı.");
            }

            if (order.OrderStatus != OrderStatusType.Tamamlandi && order.OrderStatus != OrderStatusType.OnayBeklemede)
            {
                return BadRequest("Sadece tamamlanmış veya onay bekleyen siparişler iptal edilebilir.");
            }

            var customer = await _customerRepository.GetByCustomer(order.CustomerID);
            var productStock = await _productRepository.GetProductStock(order.ProductID);

            if (order.OrderStatus == OrderStatusType.Tamamlandi)
            {
                var stockGuncel = new UpdateStockDto
                {
                    ProductID = order.ProductID,
                    Stock = productStock + order.Quantity 
                };
                _productRepository.UpdateProductStock(stockGuncel);

                await _customerRepository.UpdateCustomerBudgetAndTotalSpent(
                    customer.CustomerID,
                    -order.TotalPrice 
                );
            }

            await _orderRepository.UpdateOrderStatus(id, OrderStatusType.IptalEdildi);
            string logDetails;

            if (order.OrderStatus == OrderStatusType.Tamamlandi)
            {
                logDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductID} id'ye sahip üründen {order.Quantity} adet tamamlanmış siparişi iptal edildi. Bütçe iade edildi ve stok güncellendi.";
            }
            else
            {
                logDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductID} id'ye sahip üründen {order.Quantity} adet onay bekleyen siparişi iptal etti.";
            }

            await _logRepository.AddLog(new CreateLogDto
            {
                CustomerID = order.CustomerID,
                OrderID = order.OrderID,
                LogDate = DateTime.Now,
                LogType = "Uyarı",
                LogDetails = logDetails
            });
           
            return Ok("Sipariş iptal edildi.");
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> OrderGoruntuleme(int orderId)
        {
            var siparis = await _orderRepository.GetOrder(orderId);
            if (siparis == null)
            {
                return NotFound("Sipariş bulunamadı.");
            }
            return Ok(siparis);
        }
        [HttpGet("Customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerId(int customerId)
        {
            var siparis = await _orderRepository.GetOrdersByCustomerId(customerId);
            if (siparis.Count == 0)
            {
                return NotFound("Müşteriye ait sipariş bulunamadı.");
            }
            return Ok(siparis);
        }
        [HttpPut]
        public async Task<IActionResult> OrderGuncelle( UpdateOrderDto order)
        {
            _orderRepository.UpdateOrder(order);
            return Ok("Sipariş başarıyla güncellendi.");
        }
        [HttpPut("DurumGuncelleme/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatusType status)
        {
            await _orderRepository.UpdateOrderStatus(orderId, status);
            return Ok($"Sipariş ID {orderId} durumu başarıyla güncellendi.");
        }
       
        [HttpGet("OnayliSiparisler")]
        public async Task<IActionResult> GetOnayliSiparisler()
        {
            
            var tumSiparisler = await _orderRepository.GetOnayliSiparisler();
            if (tumSiparisler  == null ||  tumSiparisler.Count == 0)
            {
                return NotFound("Hiç sipariş bulunamadı.");
            }

            return Ok (tumSiparisler);


        }
        [HttpPut("UpdateOncelikBeklemeSuresi/{orderId}")]
        public async Task<IActionResult> UpdateOncelikBeklemeSuresi(int orderId,UpdateOncelikSkorBeklemeSureDtos guncel)
        {
            await _orderRepository.UpdateOncelikBeklemeSuresi(orderId, guncel.BeklemeSuresi, guncel.OncelikSkoru, guncel.OrderStatus);

            return Ok($"Sipariş ID {orderId} öncelik skoru ve bekleme süresi başarıyla güncellendi.");

        }
        //[HttpPost]
        //public async Task<IActionResult> SiparisIsleme()
        //{

        //        await _order.OnayliOrderIslemleri();


        //        return Ok("Onaylı siparişlerin işlenmesi tamamlandı.");

        //}

        private readonly ConcurrentDictionary<int, Mutex> _customerMutexes = new();
        private readonly ConcurrentDictionary<int, Mutex> _productMutexes = new();
        [HttpPost("SiparisIsleme")]
        public async Task<IActionResult> SiparisIsleme()
        {
            var siparisSirasiList = await _orderRepository.GetOnayliSiparisler();
            if (!siparisSirasiList.Any())
            {
                return Ok("Bekleyen sipariş bulunmamaktadır.");
            }

            var groupedOrders = siparisSirasiList
                .GroupBy(order => order.ProductID)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(order => order.OncelikSkoru).ToList()
                );

            foreach (var group in groupedOrders)
            {
                int productId = group.Key;
                var productOrders = group.Value;

                var thread = new Thread(() =>
                {
                    var productMutex = _productMutexes.GetOrAdd(productId, _ => new Mutex());

                    try
                    {
                        productMutex.WaitOne(); 

                        foreach (var order in productOrders)
                        {
                            var customerMutex = _customerMutexes.GetOrAdd(order.CustomerID, _ => new Mutex());

                            try
                            {
                                customerMutex.WaitOne(); 
                                ProcessOrder(order).Wait(); 
                            }
                            finally
                            {
                                customerMutex.ReleaseMutex(); 
                            }
                        }
                    }
                    finally
                    {
                        productMutex.ReleaseMutex(); 
                    }
                });
                //var mutex = _productMutex.GetOrAdd(productId, _ => new Mutex());

                //foreach (var order in productOrders)
                //{
                //   // SemaphoreClass.semaphore.Wait();

                //    try
                //    {
                //        mutex.WaitOne();
                //        ProcessOrder(order).Wait();
                //    }
                //    catch (Exception ex)
                //    {
                //        _logRepository.AddLog(new CreateLogDto
                //        {
                //            CustomerID = order.CustomerID,
                //            OrderID = order.OrderID,
                //            LogDate = DateTime.Now,
                //            LogType = "Hata",
                //            LogDetails = $"Sipariş ID {order.OrderID} işlenirken hata oluştu: {ex.Message}"
                //        }).Wait();
                //        Console.WriteLine($"Sipariş ID {order.OrderID} işlenirken hata: {ex.Message}");
                //    }
                //    finally
                //    {
                //        mutex.ReleaseMutex();
                //       // SemaphoreClass.semaphore.Release();
                //    }
            //}
            //    });

                thread.IsBackground = true;
                thread.Start();
            }

            return Ok("Sipariş işleme işlemi yapılıyor.");
        }
        

        private async Task ProcessOrder(ResultOrderDto order)
        {

            var customer = await _customerRepository.GetByCustomer(order.CustomerID);
            await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Isleniyor);
            Thread.Sleep(1200);
           await _logRepository.AddLog(new CreateLogDto
            {
                CustomerID = order.CustomerID,
                OrderID = order.OrderID,
                LogDate = DateTime.Now,
                LogType = "Bilgilendirme",
                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi işleme alındı."
            });
            if (SemaphoreClass.IsAdmin == true)
            {
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sırasında  admin stok güncelleme işlemi algılandı.İşlemin bitmesi bekleniyor..."
                });
                while (SemaphoreClass.IsAdmin == true)
                {
                    Thread.Sleep(500);
                }
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $" Admin stok güncelleme işlemi tamamlandı.Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişine devam ediliyor..."
                });
            }

            if (SemaphoreClass.IsAdminDelete == true)
            {
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sırasında  admin ürün silme işlemi algılandı.İşlemin bitmesi bekleniyor..."
                });
                while (SemaphoreClass.IsAdminDelete == true)
                {
                    Thread.Sleep(500);
                }
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $" Admin stok güncelleme işlemi tamamlandı.Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişine devam ediliyor..."
                });
            }
            var product = await _productRepository.GetProduct(order.ProductID);
            if (product== null)
            {
                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Uyarı",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi ürünün silinmesi nedeniyle iptal edildi..."
                });
                return;
            }
            var productStock = await _productRepository.GetProductStock(order.ProductID);

            if (productStock < order.Quantity)
            {
                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Uyarı",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz stok nedeniyle iptal edildi. Stok adeti: {productStock}."
                });
                return;
            }

            if (customer.Budget < order.TotalPrice)
            {
                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.ButceYetersizligi);
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz bütçe nedeniyle iptal edildi. Toplam Tutar: {order.TotalPrice:C}, Bütçe: {customer.Budget:C}."
                });
                return;
            }
            var stockGuncel = new UpdateStockDto
            {
                ProductID = order.ProductID,
                Stock = Math.Max(0, productStock - order.Quantity),
            };
            _productRepository.UpdateProductStock(stockGuncel);
            await _logRepository.AddLog(new CreateLogDto
            {
                CustomerID = order.CustomerID,
                OrderID = order.OrderID,
                LogDate = DateTime.Now,
                LogType = "Bilgilendirme",
                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası stok güncellendi.Yeni stok:{stockGuncel.Stock}"
            });

            await _customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, order.TotalPrice);
            customer = await _customerRepository.GetByCustomer(order.CustomerID);
            await _logRepository.AddLog(new CreateLogDto
            {
                CustomerID = order.CustomerID,
                OrderID = order.OrderID,
                LogDate = DateTime.Now,
                LogType = "Bilgilendirme",
                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası Toplam harcama: {customer.TotalSpent} Bütçe:{customer.Budget}."
            });

            if (customer.CustomerType == "Standart" && customer.TotalSpent > 2000)
            {
                await _customerRepository.UpdateCustomerType(customer.CustomerID, "Premium");
                await _logRepository.AddLog(new CreateLogDto
                {
                    CustomerID = order.CustomerID,
                    OrderID = order.OrderID,
                    LogDate = DateTime.Now,
                    LogType = "Bilgilendirme",
                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası toplam harcama: {customer.TotalSpent}. Premium müşteri yapıldı."
                });
            }

            await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Tamamlandi);
            await _logRepository.AddLog(new CreateLogDto
            {
                CustomerID = order.CustomerID,
                OrderID = order.OrderID,
                LogDate = DateTime.Now,
                LogType = "Bilgilendirme",
                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi tamamlandı."
            });
        }


        //private async Task ProcessOrder(ResultOrderDto order)
        //{

        //    var customer = await _customerRepository.GetByCustomer(order.CustomerID);
        //    await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Isleniyor);
        //    Thread.Sleep(1200);
        //    await _logRepository.AddLog(new CreateLogDto
        //    {
        //        CustomerID = order.CustomerID,
        //        OrderID = order.OrderID,
        //        LogDate = DateTime.Now,
        //        LogType = "Bilgilendirme",
        //        LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi işleme alındı."
        //    });
        //    if (SemaphoreClass.IsAdmin == true)
        //    {
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sırasında  admin stok güncelleme işlemi algılandı.İşlemin bitmesi bekleniyor..."
        //        });
        //        while (SemaphoreClass.IsAdmin == true)
        //        {
        //            Thread.Sleep(500); 
        //        }
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $" Admin stok güncelleme işlemi tamamlandı.Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişine devam ediliyor..."
        //        });
        //    }

        //    if (SemaphoreClass.IsAdminDelete == true)
        //    {
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sırasında  admin ürün silme işlemi algılandı.İşlemin bitmesi bekleniyor..."
        //        });
        //        while (SemaphoreClass.IsAdminDelete == true)
        //        {
        //            Thread.Sleep(500);
        //        }
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $" Admin stok güncelleme işlemi tamamlandı.Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişine devam ediliyor..."
        //        });
        //    }
        //    var product = await _productRepository.GetProduct(order.ProductID);
        //    if (product== null)
        //    {
        //        await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Uyarı",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi ürünün silinmesi nedeniyle iptal edildi..."
        //        });
        //        return;
        //    }
        //    var productStock = await _productRepository.GetProductStock(order.ProductID);

        //    if (productStock < order.Quantity)
        //    {
        //        await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Uyarı",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz stok nedeniyle iptal edildi. Stok adeti: {productStock}."
        //        });
        //        return;
        //    }

        //    if (customer.Budget < order.TotalPrice)
        //    {
        //        await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.ButceYetersizligi);
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz bütçe nedeniyle iptal edildi. Toplam Tutar: {order.TotalPrice:C}, Bütçe: {customer.Budget:C}."
        //        });
        //        return;
        //    }
        //    var stockGuncel = new UpdateStockDto
        //    {
        //        ProductID = order.ProductID,
        //        Stock = Math.Max(0, productStock - order.Quantity),
        //    };
        //    _productRepository.UpdateProductStock(stockGuncel);
        //    await _logRepository.AddLog(new CreateLogDto
        //    {
        //        CustomerID = order.CustomerID,
        //        OrderID = order.OrderID,
        //        LogDate = DateTime.Now,
        //        LogType = "Bilgilendirme",
        //        LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası stok güncellendi.Yeni stok:{stockGuncel.Stock}"
        //    });

        //    await _customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, order.TotalPrice);
        //    customer = await _customerRepository.GetByCustomer(order.CustomerID);
        //    await _logRepository.AddLog(new CreateLogDto
        //    {
        //        CustomerID = order.CustomerID,
        //        OrderID = order.OrderID,
        //        LogDate = DateTime.Now,
        //        LogType = "Bilgilendirme",
        //        LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası Toplam harcama: {customer.TotalSpent} Bütçe:{customer.Budget}."
        //    });

        //    if (customer.CustomerType == "Standart" && customer.TotalSpent > 2000)
        //    {
        //        await _customerRepository.UpdateCustomerType(customer.CustomerID, "Premium");
        //        await _logRepository.AddLog(new CreateLogDto
        //        {
        //            CustomerID = order.CustomerID,
        //            OrderID = order.OrderID,
        //            LogDate = DateTime.Now,
        //            LogType = "Bilgilendirme",
        //            LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası toplam harcama: {customer.TotalSpent}. Premium müşteri yapıldı."
        //        });
        //    }

        //    await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Tamamlandi);
        //    await _logRepository.AddLog(new CreateLogDto
        //    {
        //        CustomerID = order.CustomerID,
        //        OrderID = order.OrderID,
        //        LogDate = DateTime.Now,
        //        LogType = "Bilgilendirme",
        //        LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi tamamlandı."
        //    });

        //}

    }
}




