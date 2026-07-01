using BookingBakery.Application.DTO;
using System.IO;

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
        Task<CategoryDto?> UpdateCategoryImageAsync(int id, Stream imageStream, string fileName);
        Task<CategoryDto?> UpdateCategoryImageByNameAsync(string name, Stream imageStream, string fileName);
    }
}
