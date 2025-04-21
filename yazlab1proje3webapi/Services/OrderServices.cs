using System.Collections.Concurrent;
using System.Text;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.LogDtos;
using yazlab1proje3webapi.Dtos.OrderDtos;
using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Repositories.CustomerRepositories;
using yazlab1proje3webapi.Repositories.LogRepositories;
using yazlab1proje3webapi.Repositories.OrderRepositories;
using yazlab1proje3webapi.Repositories.ProductRepositories;

namespace yazlab1proje3webapi.Services
{
    public class OrderServices:IOrderServices
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogRepository _logRepository;

        private static readonly ConcurrentDictionary<int, Mutex> _productMutex = new();
        public OrderServices(IOrderRepository orderRepository, IProductRepository productRepository, ICustomerRepository customerRepository, ILogRepository logRepository)
        {
            _orderRepository=orderRepository;
            _productRepository=productRepository;
            _customerRepository=customerRepository;
            _logRepository=logRepository;
        }


        public async Task OnayliOrderIslemleri()
        {
            var siparisSirasiList = _orderRepository.GetOnayliSiparisler().Result;
            if (!siparisSirasiList.Any())
            {
                Console.WriteLine("Bekleyen sipariş kalmadı.");
                return;
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
                    var mutex = _productMutex.GetOrAdd(productId, _ => new Mutex());

                    foreach (var order in productOrders)
                    {
                        SemaphoreClass.semaphore.Wait();

                        try
                        {
                            mutex.WaitOne();
                            var customer = _customerRepository.GetByCustomer(order.CustomerID).Result;

                            _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Isleniyor).Wait();
                            _logRepository.AddLog(new CreateLogDto
                            {
                                CustomerID = order.CustomerID,
                                OrderID = order.OrderID,
                                LogDate = DateTime.Now,
                                LogType = "Bilgilendirme",
                                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi işleme alındı."
                            });
                            var productStock = _productRepository.GetProductStock(order.ProductID).Result;

                            if (productStock < order.Quantity)
                            {
                                _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi).Wait();
                                _logRepository.AddLog(new CreateLogDto
                                {
                                    CustomerID = order.CustomerID,
                                    OrderID = order.OrderID,
                                    LogDate = DateTime.Now,
                                    LogType = "Uyarı",
                                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz stok nedeniyle iptal edildi.Stok adeti:{productStock}"
                                });
                                Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz stok nedeniyle iptal edildi.");
                                continue;
                            }

                            var toplamTutar = order.TotalPrice;

                            if (customer.Budget < toplamTutar)
                            {
                                _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.ButceYetersizligi).Wait();
                                _logRepository.AddLog(new CreateLogDto
                                {
                                    CustomerID = order.CustomerID,
                                    OrderID = order.OrderID,
                                    LogDate = DateTime.Now,
                                    LogType = "Uyarı",
                                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi yetersiz bütçe nedeniyle iptal edildi.Toplam Tutar: {order.TotalPrice:C}, Bütçe: {customer.Budget:C}"
                                });
                                Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz bütçe nedeniyle iptal edildi.");
                                continue;
                            }

                            var stockGuncel = new UpdateStockDto
                            {
                                ProductID = order.ProductID,
                                Stock = Math.Max(0, productStock - order.Quantity),
                            };
                            _productRepository.UpdateProductStock(stockGuncel);

                            _customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, toplamTutar).Wait();
                            if (customer.CustomerType=="Standart" && customer.TotalSpent>2000)
                            {
                                _customerRepository.UpdateCustomerType(customer.CustomerID, "Premium").Wait();
                                _logRepository.AddLog(new CreateLogDto
                                {
                                    CustomerID = order.CustomerID,
                                    OrderID = order.OrderID,
                                    LogDate = DateTime.Now,
                                    LogType = "Bilgilendirme",
                                    LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi sonrası toplam harcama: {customer.TotalSpent}.Premium müşteri yapılıyor."
                                });
                            }
                            _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Tamamlandi).Wait();
                            _logRepository.AddLog(new CreateLogDto
                            {
                                CustomerID = order.CustomerID,
                                OrderID = order.OrderID,
                                LogDate = DateTime.Now,
                                LogType = "Bilgilendirme",
                                LogDetails = $"Müşteri {customer.CustomerName} ({customer.CustomerType}) {order.ProductName}'den {order.Quantity} adet siparişi tamamlandı."
                            });
                            Console.WriteLine($"Sipariş ID {order.OrderID}: Başarıyla işlendi. Kalan bütçe: {customer.Budget} TL");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Sipariş ID {order.OrderID} işlenirken hata: {ex.Message}");
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                            SemaphoreClass.semaphore.Release();
                        }
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            }

            Console.WriteLine("Tüm sipariş işlemleri başlatıldı. Tamamlanan işlemler anında görüntülenecektir.");
        }

        //public async Task OnayliOrderIslemleri()
        //{
        //    var siparisSirasiList = await _orderRepository.GetOnayliSiparisler();
        //    if (!siparisSirasiList.Any())
        //    {
        //        Console.WriteLine("Bekleyen sipariş kalmadı.");
        //        return;
        //    }

        //    // Siparişleri ürün bazında gruplandır ve öncelik sırasına göre sırala
        //    var groupedOrders = siparisSirasiList
        //        .GroupBy(order => order.ProductID)
        //        .ToDictionary(
        //            group => group.Key,
        //            group => group.OrderByDescending(order => order.OncelikSkoru).ToList()
        //        );

        //    // Her grup için paralel çalıştırılacak görevler
        //    var tasks = groupedOrders.Select(async group =>
        //    {
        //        int productId = group.Key;
        //        var productOrders = group.Value;

        //        var mutex = _productMutex.GetOrAdd(productId, _ => new Mutex());

        //        foreach (var order in productOrders)
        //        {
        //            await SemaphoreClass.semaphore.WaitAsync();

        //            try
        //            {
        //                mutex.WaitOne(); // Ürün için kilidi al

        //                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Isleniyor);
        //                var productStock = await _productRepository.GetProductStock(order.ProductID);

        //                if (productStock < order.Quantity)
        //                {
        //                    await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
        //                    Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz stok nedeniyle iptal edildi.");
        //                    continue;
        //                }

        //                var customer = await _customerRepository.GetByCustomer(order.CustomerID);
        //                var toplamTutar = order.TotalPrice;

        //                if (customer.Budget < toplamTutar)
        //                {
        //                    await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.ButceYetersizligi);
        //                    Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz bütçe nedeniyle iptal edildi.");
        //                    continue;
        //                }

        //                // Stok güncelleme
        //                var stockGuncel = new UpdateStockDto
        //                {
        //                    ProductID = order.ProductID,
        //                    Stock = Math.Max(0, productStock - order.Quantity),
        //                };
        //                _productRepository.UpdateProductStock(stockGuncel);

        //                // Bütçe güncelleme
        //                await _customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, toplamTutar);
        //_customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, toplamTutar).Wait();
        // if(customer.TotalSpent>2000)
        // {
        //      _customerRepository.UpdateCustomerType(customer.CustomerID, "Prime");
        // }
        //                // Siparişi tamamla
        //                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Tamamlandi);

        //                Console.WriteLine($"Sipariş ID {order.OrderID}: Başarıyla işlendi. Kalan bütçe: {customer.Budget} TL");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Sipariş ID {order.OrderID} işlenirken hata: {ex.Message}");
        //            }
        //            finally
        //            {
        //                mutex.ReleaseMutex(); // Kilidi serbest bırak
        //                SemaphoreClass.semaphore.Release(); // Semaphore'u serbest bırak
        //            }
        //        }
        //    });

        //    // Tüm görevlerin tamamlanmasını bekle
        //    await Task.WhenAll(tasks);

        //    Console.WriteLine("Tüm sipariş işlemleri tamamlandı.");
        //}

        //public async Task OnayliOrderIslemleri()
        //{
        //    var siparisSirasiList = await _orderRepository.GetOnayliSiparisler();
        //    if (!siparisSirasiList.Any())
        //    {
        //        Console.WriteLine("Bekleyen sipariş kalmadı.");
        //        return;
        //    }
        //    foreach (var order in siparisSirasiList)
        //    {
        //        await SemaphoreClass.semaphore.WaitAsync();
        //        try
        //        {
        //            await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Isleniyor);
        //            var productStock = await _productRepository.GetProductStock(order.ProductID);
        //            if (productStock < order.Quantity)
        //            {
        //                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.IptalEdildi);
        //                Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz stok nedeniyle iptal edildi.");
        //                continue;
        //            }

        //            var customer = await _customerRepository.GetByCustomer(order.CustomerID);
        //            var toplamTutar = order.TotalPrice;
        //            if (customer.Budget <  toplamTutar)
        //            {
        //                await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.ButceYetersizligi);
        //                Console.WriteLine($"Sipariş ID {order.OrderID}: Yetersiz bütçe nedeniyle iptal edildi.");
        //                continue;
        //            }
        //            var stockGuncel = new UpdateStockDto
        //            {
        //                ProductID = order.ProductID,
        //                Stock = Math.Max(0, productStock - order.Quantity),
        //            };

        //            _productRepository.UpdateProductStock(stockGuncel);

        //            await _customerRepository.UpdateCustomerBudgetAndTotalSpent(customer.CustomerID, toplamTutar);

        //            await _orderRepository.UpdateOrderStatus(order.OrderID, OrderStatusType.Tamamlandi);

        //            Console.WriteLine($"Sipariş ID {order.OrderID}: Başarıyla işlendi. Kalan bütçe: {customer.Budget} TL");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Sipariş ID {order.OrderID} işlenirken hata: {ex.Message}");
        //        }
        //    }
        //}

    }
}
