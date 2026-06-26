using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IProductIngredientService
    {
        Task<ProductIngredientDto> AddProductIngredientAsync(CreateProductIngredientDto dto);
        Task<ProductIngredientDto?> UpdateProductIngredientAsync(string productName, string ingredientName, UpdateProductIngredientDto dto);
        Task<ProductRecipeDto> GetIngredientsByProductNameAsync(string productName);
        Task<bool> DeleteProductIngredientAsync(string productName, string ingredientName);
    }
}
