using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> AddCategoryAsync(CreateCategoryDto dto)
        {
            // Kiểm tra trùng tên danh mục
            var existingCategory = await _categoryRepository.FindOneAsync(c => c.Name.ToLower() == dto.Name.ToLower());
            if (existingCategory != null)
                throw new InvalidOperationException("Tên danh mục đã tồn tại.");

            var all = await _categoryRepository.GetAllAsync();
            var nextId = all.Any() ? all.Max(c => c.CategoryId) + 1 : 1;

            var category = new Category
            {
                CategoryId = nextId,
                Name = dto.Name,
                Description = dto.Description
            };

            await _categoryRepository.CreateAsync(category);

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == id);
            if (category == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                category.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                category.Description = dto.Description;
            }

            await _categoryRepository.UpdateAsync(c => c.CategoryId == id, category);

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == id);
            if (category == null)
                return false;

            await _categoryRepository.DeleteAsync(c => c.CategoryId == id);
            return true;
        }

        public async Task<bool> DeleteCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.Name.ToLower() == name.ToLower());
            if (category == null)
                return false;

            await _categoryRepository.DeleteAsync(c => c.CategoryId == category.CategoryId);
            return true;
        }

        public async Task<CategoryDto?> UpdateCategoryByNameAsync(string name, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.Name.ToLower() == name.ToLower());
            if (category == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(category.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _categoryRepository.FindOneAsync(c => c.Name.ToLower() == dto.Name.ToLower());
                if (existing != null)
                    throw new InvalidOperationException("Tên danh mục mới đã tồn tại.");

                category.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                category.Description = dto.Description;
            }

            await _categoryRepository.UpdateAsync(c => c.CategoryId == category.CategoryId, category);

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
