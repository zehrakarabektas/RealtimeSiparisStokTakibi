using yazlab1proje3webapi.Dtos.SepetimDtos;

namespace yazlab1proje3webapi.Repositories.SepetimRepositories
{
    public interface ISepetimRepository
    {
        // Kullanıcının sepetini alır
        Task<IEnumerable<ResultSepetimDtos>> GetCartByCustomerId(int customerId);

        // Sepete ürün ekler
        Task<bool> AddSepetim(CreateSepetimDto urun);

        // Sepetten ürün kaldırır
        Task<bool> DeleteSepetim(int sepetimId);

        // Sepeti tamamen boşaltır
        Task<bool> AllDeleteSepetim(int customerId);

        // Sepetteki toplam ürün sayısını alır
        Task<int> GetSepetimUrunSay(int customerId);
    }
}
