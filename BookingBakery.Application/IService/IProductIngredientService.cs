using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IProductIngredientService
    {
        Task<ProductIngredientDto> AddProductIngredientAsync(CreateProductIngredientDto dto);
        Task<ProductIngredientDto?> UpdateProductIngredientAsync(string productName, string ingredientName, UpdateProductIngredientDto dto);
        Task<ProductRecipeDto> GetIngredientsByProductNameAsync(string productName);
        Task<bool> DeleteProductIngredientAsync(string productName, string ingredientName);

        Task<ProductIngredientDto> AddProductIngredientByIdAsync(CreateProductIngredientByIdDto dto);
        Task<ProductIngredientDto?> UpdateProductIngredientByIdAsync(int productId, int ingredientId, UpdateProductIngredientDto dto);
        Task<ProductRecipeDto> GetIngredientsByProductIdAsync(int productId);
        Task<bool> DeleteProductIngredientByIdAsync(int productId, int ingredientId);
    }
}
