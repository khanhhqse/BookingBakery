using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync();
        Task<IEnumerable<IngredientDto>> GetIngredientsInStockAsync();
        Task<IEnumerable<IngredientDto>> GetIngredientsSoldOutAsync();
        Task<IEnumerable<IngredientDto>> GetLowStockIngredientsAsync();
        Task<IngredientDto?> GetIngredientByNameAsync(string name);
        Task<IngredientDto> AddIngredientAsync(CreateIngredientDto dto);
        Task<IngredientDto?> UpdateStockByNameAsync(string name, decimal quantity);
        Task<IngredientDto?> UpdateCostByNameAsync(string name, decimal cost);
        Task<IngredientDto?> UpdateCostByIdAsync(int id, decimal cost);
        Task<IngredientDto?> UpdateIngredientAsync(int id, UpdateIngredientDto dto);
        Task<IngredientDto?> UpdateIngredientByNameAsync(string name, UpdateIngredientDto dto);
        Task<bool> DeleteIngredientByNameAsync(string name);
        Task<bool> DeleteIngredientByIdAsync(int id);
    }
}
