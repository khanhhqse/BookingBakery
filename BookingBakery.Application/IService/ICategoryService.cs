using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> AddCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> DeleteCategoryByNameAsync(string name);
        Task<CategoryDto?> UpdateCategoryByNameAsync(string name, UpdateCategoryDto dto);
    }
}
