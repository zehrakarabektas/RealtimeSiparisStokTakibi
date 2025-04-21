using yazlab1proje3webapi.Dtos.ProductDtos;
using yazlab1proje3webapi.Dtos.VeriDtos;

namespace yazlab1proje3webapi.Repositories.VeriRepositories
{
    public interface IVeriRepository
    {
        int ProductCount();
        int CustomerCount();
        int OrderCount();
        Task<List<ProductSalesDtos>> GetProductSales();
        Task<RevenueSummary> GetRevenueSummaryAsync();
        Task<List<CityCustomerCount>> GetCustomerCountsByCityAsync();
    }
}
