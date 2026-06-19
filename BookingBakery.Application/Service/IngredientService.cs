using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            return ingredients.Select(MapToDto);
        }

        public async Task<IEnumerable<IngredientDto>> GetIngredientsInStockAsync()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            // Lọc ra các nguyên liệu còn tồn kho (current_stock > 0)
            return ingredients.Where(i => i.CurrentStock > 0).Select(MapToDto);
        }

        public async Task<IEnumerable<IngredientDto>> GetIngredientsSoldOutAsync()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            // Lọc ra các nguyên liệu đã hết tồn kho (current_stock <= 0)
            return ingredients.Where(i => i.CurrentStock <= 0).Select(MapToDto);
        }

        public async Task<IEnumerable<IngredientDto>> GetLowStockIngredientsAsync()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            // Lọc ra các nguyên liệu sắp hết tồn kho (current_stock <= 10)
            return ingredients.Where(i => i.CurrentStock <= 10).Select(MapToDto);
        }

        public async Task<IngredientDto?> GetIngredientByNameAsync(string name)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == name.ToLower());
            if (ingredient == null) return null;
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto> AddIngredientAsync(CreateIngredientDto dto)
        {
            // Kiểm tra trùng tên nguyên liệu
            var existing = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == dto.Name.ToLower());
            if (existing != null)
                throw new InvalidOperationException($"Nguyên liệu '{dto.Name}' đã tồn tại.");

            if (dto.CurrentStock <= 0)
                throw new InvalidOperationException("Số lượng tồn kho phải lớn hơn 0.");

            if (dto.CostPerUnit <= 0)
                throw new InvalidOperationException("Đơn giá phải lớn hơn 0.");

            var all = await _ingredientRepository.GetAllAsync();
            var nextId = all.Any() ? all.Max(i => i.IngredientId) + 1 : 1;

            var ingredient = new Ingredient
            {
                IngredientId = nextId,
                Name = dto.Name,
                Unit = dto.Unit,
                CurrentStock = dto.CurrentStock,
                CostPerUnit = dto.CostPerUnit,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _ingredientRepository.CreateAsync(ingredient);
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto?> UpdateStockByNameAsync(string name, decimal quantity)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == name.ToLower());
            if (ingredient == null) return null;

            ingredient.CurrentStock = quantity;
            if (ingredient.CreatedAt == default)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
            }
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _ingredientRepository.UpdateAsync(i => i.IngredientId == ingredient.IngredientId, ingredient);
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto?> UpdateCostByNameAsync(string name, decimal cost)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == name.ToLower());
            if (ingredient == null) return null;

            ingredient.CostPerUnit = cost;
            if (ingredient.CreatedAt == default)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
            }
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _ingredientRepository.UpdateAsync(i => i.IngredientId == ingredient.IngredientId, ingredient);
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto?> UpdateCostByIdAsync(int id, decimal cost)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.IngredientId == id);
            if (ingredient == null) return null;

            ingredient.CostPerUnit = cost;
            if (ingredient.CreatedAt == default)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
            }
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _ingredientRepository.UpdateAsync(i => i.IngredientId == id, ingredient);
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto?> UpdateIngredientAsync(int id, UpdateIngredientDto dto)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.IngredientId == id);
            if (ingredient == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(ingredient.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == dto.Name.ToLower());
                if (existing != null)
                    throw new InvalidOperationException($"Nguyên liệu '{dto.Name}' đã tồn tại.");

                ingredient.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Unit))
            {
                ingredient.Unit = dto.Unit;
            }

            if (dto.CurrentStock.HasValue)
            {
                ingredient.CurrentStock = dto.CurrentStock.Value;
            }

            if (dto.CostPerUnit.HasValue)
            {
                ingredient.CostPerUnit = dto.CostPerUnit.Value;
            }

            if (ingredient.CreatedAt == default)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
            }
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _ingredientRepository.UpdateAsync(i => i.IngredientId == id, ingredient);
            return MapToDto(ingredient);
        }

        public async Task<IngredientDto?> UpdateIngredientByNameAsync(string name, UpdateIngredientDto dto)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == name.ToLower());
            if (ingredient == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(ingredient.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == dto.Name.ToLower());
                if (existing != null)
                    throw new InvalidOperationException($"Nguyên liệu '{dto.Name}' đã tồn tại.");

                ingredient.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Unit))
            {
                ingredient.Unit = dto.Unit;
            }

            if (dto.CurrentStock.HasValue)
            {
                ingredient.CurrentStock = dto.CurrentStock.Value;
            }

            if (dto.CostPerUnit.HasValue)
            {
                ingredient.CostPerUnit = dto.CostPerUnit.Value;
            }

            if (ingredient.CreatedAt == default)
            {
                ingredient.CreatedAt = DateTime.UtcNow;
            }
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _ingredientRepository.UpdateAsync(i => i.IngredientId == ingredient.IngredientId, ingredient);
            return MapToDto(ingredient);
        }

        public async Task<bool> DeleteIngredientByNameAsync(string name)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == name.ToLower());
            if (ingredient == null) return false;

            await _ingredientRepository.DeleteAsync(i => i.IngredientId == ingredient.IngredientId);
            return true;
        }

        public async Task<bool> DeleteIngredientByIdAsync(int id)
        {
            var ingredient = await _ingredientRepository.FindOneAsync(i => i.IngredientId == id);
            if (ingredient == null) return false;

            await _ingredientRepository.DeleteAsync(i => i.IngredientId == id);
            return true;
        }

        private static IngredientDto MapToDto(Ingredient i)
        {
            return new IngredientDto
            {
                IngredientId = i.IngredientId,
                Name = i.Name,
                Unit = i.Unit,
                CurrentStock = i.CurrentStock,
                CostPerUnit = i.CostPerUnit,
                Status = i.CurrentStock > 0 ? "stock" : "sold_out",
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            };
        }
    }
}
