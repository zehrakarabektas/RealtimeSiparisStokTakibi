using yazlab1proje3webapi.Dtos.ProductDtos;

namespace yazlab1proje3webapi.Repositories.ProductRepositories
{
    public interface IProductRepository
    {
        Task<List<ResultProductDto>> GetAllProduct();
        Task<GetByIdProductDto> GetProduct(int productId);
        void AddProduct(CreateProductDto product);   
        void UpdateProduct(UpdateProductDto product);
        void UpdateProductStock(UpdateStockDto product);
        void DeleteProduct(int productId);
        Task<int> GetProductStock(int productId);

    }
}
