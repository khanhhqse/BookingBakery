using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Helper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BookingBakery.Application.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly HelperCloudinary _cloudinaryHelper;
        private readonly IPromotionPriceHelper _promotionPriceHelper;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            HelperCloudinary cloudinaryHelper,
            IPromotionPriceHelper promotionPriceHelper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cloudinaryHelper = cloudinaryHelper;
            _promotionPriceHelper = promotionPriceHelper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var result = new List<ProductDto>();
            foreach (var p in products)
            {
                var categoryName = categoryMap.TryGetValue(p.CategoryId, out var name)
                    ? name : "Không xác định";
                result.Add(await MapToDtoAsync(p, categoryName));
            }
            return result;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            if (p == null) return null;

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto> AddProductAsync(CreateProductDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                throw new ArgumentException("Hình ảnh sản phẩm là bắt buộc.");

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {dto.CategoryId} không tồn tại.");

            var all = await _productRepository.GetAllAsync();
            var sizeUpper = dto.SizeName.Trim().ToUpper();
            var nameTrimmed = dto.Name.Trim();

            var isDuplicated = all.Any(p =>
                p.Name.Equals(nameTrimmed, StringComparison.OrdinalIgnoreCase) &&
                p.SizeName.Equals(sizeUpper, StringComparison.OrdinalIgnoreCase));

            if (isDuplicated)
                throw new InvalidOperationException($"Sản phẩm '{nameTrimmed}' với size '{sizeUpper}' đã tồn tại trong kho.");

            using var imageStream = dto.Image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.Image.FileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            var nextId = all.Any() ? all.Max(p => p.ProductId) + 1 : 1;

            var product = new Product
            {
                ProductId = nextId,
                CategoryId = dto.CategoryId,
                Name = nameTrimmed,
                SizeName = sizeUpper,
                Description = dto.Description,
                StorageInstructions = dto.StorageInstructions?.Trim(),
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                StockQuantity = dto.StockQuantity,
                ImageUrl = uploadResult.SecureUrl.ToString(),
                Status = "stock",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(product);
            return await MapToDtoAsync(product, category.Name);
        }

        public async Task<ProductDto?> UpdateStockAsync(int id, int quantity)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            p.StockQuantity = quantity;
            p.Status = quantity > 0 ? "stock" : "sold_out";
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdatePriceAsync(int id, decimal price)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            p.Price = price;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateDescriptionAsync(int id, string? description)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            p.Description = description;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateStorageInstructionsAsync(int id, string? storageInstructions)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            p.StorageInstructions = storageInstructions?.Trim();
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateStockByNameAsync(string name, int quantity)
        {
            var p = await _productRepository.FindOneAsync(x => x.Name.ToLower() == name.ToLower());
            if (p == null) return null;

            p.StockQuantity = quantity;
            p.Status = quantity > 0 ? "stock" : "sold_out";
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == p.ProductId, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdatePriceByNameAsync(string name, decimal price)
        {
            var p = await _productRepository.FindOneAsync(x => x.Name.ToLower() == name.ToLower());
            if (p == null) return null;

            p.Price = price;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == p.ProductId, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateDescriptionByNameAsync(string name, string? description)
        {
            var p = await _productRepository.FindOneAsync(x => x.Name.ToLower() == name.ToLower());
            if (p == null) return null;

            p.Description = description;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == p.ProductId, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsByNameAsync(string name)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var result = new List<ProductDto>();
            foreach (var p in products.Where(p =>
                p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
            {
                var catName = categoryMap.TryGetValue(p.CategoryId, out var n) ? n : "Không xác định";
                result.Add(await MapToDtoAsync(p, catName));
            }
            return result;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == categoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {categoryId} không tồn tại.");

            var products = await _productRepository.GetAllAsync();
            var result = new List<ProductDto>();
            foreach (var p in products.Where(p => p.CategoryId == categoryId))
                result.Add(await MapToDtoAsync(p, category.Name));

            return result;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return false;

            await _productRepository.DeleteAsync(x => x.ProductId == id);
            return true;
        }

        public async Task<ProductDto?> UpdateImageAsync(int id, Stream imageStream, string fileName)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            p.ImageUrl = uploadResult.SecureUrl.ToString();
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateImageByNameAsync(string name, Stream imageStream, string fileName)
        {
            var p = await _productRepository.FindOneAsync(x => x.Name.ToLower() == name.ToLower());
            if (p == null) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            p.ImageUrl = uploadResult.SecureUrl.ToString();
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == p.ProductId, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateNameAndCategoryAsync(int id, UpdateProductNameAndCategoryDto dto)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {dto.CategoryId} không tồn tại.");

            p.Name = dto.Name;
            p.CategoryId = dto.CategoryId;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            return await MapToDtoAsync(p, category.Name);
        }

        // ──────────────────────────────────────────────────────────────
        // CẬP NHẬT TÊN SIZE (không tạo dòng mới, chỉ đổi tên size hiện có)
        // ──────────────────────────────────────────────────────────────
        public async Task<ProductDto?> UpdateSizeNameAsync(int id, string sizeName)
        {
            var p = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (p == null) return null;

            var newSizeName = sizeName.Trim().ToUpper();

            // Không cho trùng với size khác đã có của CÙNG sản phẩm (cùng Name)
            var allProducts = await _productRepository.GetAllAsync();
            var duplicated = allProducts.Any(x =>
                x.ProductId != id &&
                x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase) &&
                x.SizeName.Equals(newSizeName, StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new InvalidOperationException($"Sản phẩm '{p.Name}' đã có size '{newSizeName}' rồi.");

            p.SizeName = newSizeName;
            p.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(x => x.ProductId == id, p);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            return await MapToDtoAsync(p, category?.Name ?? "Không xác định");
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPER
        // ──────────────────────────────────────────────────────────────

        private async Task<ProductDto> MapToDtoAsync(Product p, string categoryName)
        {
            var (salePrice, hasPromotion, promotionTitle) =
                await _promotionPriceHelper.GetSalePriceAsync(p.ProductId, p.Price);

            return new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = categoryName,
                Name = p.Name,
                SizeName = p.SizeName,
                Description = p.Description,
                StorageInstructions = p.StorageInstructions,
                Price = p.Price,
                SalePrice = salePrice,
                HasActivePromotion = hasPromotion,
                ActivePromotionTitle = promotionTitle,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}