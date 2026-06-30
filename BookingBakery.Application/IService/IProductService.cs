using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> AddProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateStockAsync(int id, int quantity);
        Task<ProductDto?> UpdatePriceAsync(int id, decimal price);
        Task<ProductDto?> UpdateDescriptionAsync(int id, string? description);
        Task<ProductDto?> UpdateStockByNameAsync(string name, int quantity);
        Task<ProductDto?> UpdatePriceByNameAsync(string name, decimal price);
        Task<ProductDto?> UpdateDescriptionByNameAsync(string name, string? description);
        Task<IEnumerable<ProductDto>> SearchProductsByNameAsync(string name);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductDto?> UpdateImageAsync(int id, Stream imageStream, string fileName);
        Task<ProductDto?> UpdateImageByNameAsync(string name, Stream imageStream, string fileName);
        Task<ProductDto?> UpdateNameAndCategoryAsync(int id, UpdateProductNameAndCategoryDto dto);
    }
}
