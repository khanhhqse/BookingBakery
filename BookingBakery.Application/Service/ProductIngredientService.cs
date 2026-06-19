using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class ProductIngredientService : IProductIngredientService
    {
        private readonly IProductIngredientRepository _productIngredientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IIngredientRepository _ingredientRepository;

        public ProductIngredientService(
            IProductIngredientRepository productIngredientRepository,
            IProductRepository productRepository,
            IIngredientRepository ingredientRepository)
        {
            _productIngredientRepository = productIngredientRepository;
            _productRepository = productRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<ProductIngredientDto> AddProductIngredientAsync(CreateProductIngredientDto dto)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == dto.ProductName.ToLower());
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm '{dto.ProductName}' không tồn tại.");

            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == dto.IngredientName.ToLower());
            if (ingredient == null)
                throw new InvalidOperationException($"Nguyên liệu '{dto.IngredientName}' không tồn tại.");

            // Kiểm tra xem đã tồn tại mối quan hệ chưa
            var existing = await _productIngredientRepository.FindOneAsync(pi => pi.ProductId == product.ProductId && pi.IngredientId == ingredient.IngredientId);
            if (existing != null)
                throw new InvalidOperationException($"Nguyên liệu '{dto.IngredientName}' đã tồn tại trong sản phẩm '{dto.ProductName}'. Vui lòng dùng tính năng cập nhật.");

            if (dto.QuantityRequired <= 0)
                throw new InvalidOperationException("Số lượng yêu cầu phải lớn hơn 0.");

            var productIngredient = new ProductIngredient
            {
                ProductId = product.ProductId,
                IngredientId = ingredient.IngredientId,
                QuantityRequired = dto.QuantityRequired
            };

            await _productIngredientRepository.CreateAsync(productIngredient);
            
            var newCostPrice = await RecalculateProductCostPriceAsync(product);

            return new ProductIngredientDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                IngredientId = ingredient.IngredientId,
                IngredientName = ingredient.Name,
                QuantityRequired = productIngredient.QuantityRequired,
                Unit = ingredient.Unit,
                CurrentStock = ingredient.CurrentStock,
                ProductCostPrice = newCostPrice
            };
        }

        public async Task<ProductIngredientDto?> UpdateProductIngredientAsync(string productName, string ingredientName, UpdateProductIngredientDto dto)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == productName.ToLower());
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm '{productName}' không tồn tại.");

            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == ingredientName.ToLower());
            if (ingredient == null)
                throw new InvalidOperationException($"Nguyên liệu '{ingredientName}' không tồn tại.");

            var productIngredient = await _productIngredientRepository.FindOneAsync(pi => pi.ProductId == product.ProductId && pi.IngredientId == ingredient.IngredientId);
            if (productIngredient == null)
                return null;

            if (dto.QuantityRequired <= 0)
                throw new InvalidOperationException("Số lượng yêu cầu phải lớn hơn 0.");

            productIngredient.QuantityRequired = dto.QuantityRequired;

            await _productIngredientRepository.UpdateAsync(
                pi => pi.ProductId == product.ProductId && pi.IngredientId == ingredient.IngredientId,
                productIngredient);
            
            var newCostPrice = await RecalculateProductCostPriceAsync(product);

            return new ProductIngredientDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                IngredientId = ingredient.IngredientId,
                IngredientName = ingredient.Name,
                QuantityRequired = productIngredient.QuantityRequired,
                Unit = ingredient.Unit,
                CurrentStock = ingredient.CurrentStock,
                ProductCostPrice = newCostPrice
            };
        }

        public async Task<ProductRecipeDto> GetIngredientsByProductNameAsync(string productName)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == productName.ToLower());
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm '{productName}' không tồn tại.");

            var list = await _productIngredientRepository.FindManyAsync(pi => pi.ProductId == product.ProductId);
            var ingredients = await _ingredientRepository.GetAllAsync();
            var ingredientMap = ingredients.ToDictionary(i => i.IngredientId, i => i);

            decimal totalCostPrice = 0;
            var ingredientList = new List<RecipeIngredientDto>();

            foreach (var pi in list)
            {
                ingredientMap.TryGetValue(pi.IngredientId, out var ing);
                decimal costPerUnit = ing?.CostPerUnit ?? 0;
                totalCostPrice += pi.QuantityRequired * costPerUnit;

                ingredientList.Add(new RecipeIngredientDto
                {
                    IngredientName = ing?.Name ?? "Không xác định",
                    QuantityRequired = pi.QuantityRequired,
                    Unit = ing?.Unit ?? string.Empty,
                    CurrentStock = ing?.CurrentStock ?? 0
                });
            }

            // Đồng bộ lại giá vốn của sản phẩm trong MongoDB nếu có sự khác biệt (ví dụ dữ liệu cũ chưa tính)
            if (product.CostPrice != totalCostPrice)
            {
                product.CostPrice = totalCostPrice;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);
            }

            return new ProductRecipeDto
            {
                ProductName = product.Name,
                ProductCostPrice = totalCostPrice,
                Ingredients = ingredientList
            };
        }

        public async Task<bool> DeleteProductIngredientAsync(string productName, string ingredientName)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == productName.ToLower());
            if (product == null) return false;

            var ingredient = await _ingredientRepository.FindOneAsync(i => i.Name.ToLower() == ingredientName.ToLower());
            if (ingredient == null) return false;

            var existing = await _productIngredientRepository.FindOneAsync(pi => pi.ProductId == product.ProductId && pi.IngredientId == ingredient.IngredientId);
            if (existing == null) return false;

            await _productIngredientRepository.DeleteAsync(pi => pi.ProductId == product.ProductId && pi.IngredientId == ingredient.IngredientId);
            
            await RecalculateProductCostPriceAsync(product);
            
            return true;
        }

        //Helper: Tính toán lại giá vốn của sản phẩm
        private async Task<decimal> RecalculateProductCostPriceAsync(Product product)
        {
            var productIngredients = await _productIngredientRepository.FindManyAsync(pi => pi.ProductId == product.ProductId);
            var ingredients = await _ingredientRepository.GetAllAsync();
            var ingredientMap = ingredients.ToDictionary(i => i.IngredientId, i => i.CostPerUnit);

            decimal totalCostPrice = 0;
            foreach (var pi in productIngredients)
            {
                if (ingredientMap.TryGetValue(pi.IngredientId, out var costPerUnit))
                {
                    totalCostPrice += pi.QuantityRequired * costPerUnit;
                }
            }

            // Tối ưu hóa: Cập nhật trực tiếp trên thực thể product đã có trong bộ nhớ
            product.CostPrice = totalCostPrice;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            return totalCostPrice;
        }
    }
}
