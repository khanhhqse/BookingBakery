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

            using var imageStream = dto.Image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.Image.FileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            var all = await _productRepository.GetAllAsync();
            var nextId = all.Any() ? all.Max(p => p.ProductId) + 1 : 1;

            var product = new Product
            {
                ProductId = nextId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                SizeName = dto.SizeName.Trim().ToUpper(),
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
        // SIZE MANAGEMENT (mỗi size = 1 dòng Product riêng, dùng chung Name)
        // ──────────────────────────────────────────────────────────────

        public async Task<ProductDto?> AddSizeAsync(int id, ProductSizeRequest request)
        {
            var baseProduct = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (baseProduct == null) return null;

            var sizeName = request.SizeName.Trim().ToUpper();

            var allProducts = await _productRepository.GetAllAsync();
            var duplicated = allProducts.Any(p =>
                p.Name.Equals(baseProduct.Name, StringComparison.OrdinalIgnoreCase) &&
                p.SizeName.Equals(sizeName, StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new InvalidOperationException($"Sản phẩm '{baseProduct.Name}' đã có size '{sizeName}'.");

            var nextId = allProducts.Any() ? allProducts.Max(p => p.ProductId) + 1 : 1;

            var newSize = new Product
            {
                ProductId = nextId,
                CategoryId = baseProduct.CategoryId,
                Name = baseProduct.Name,
                SizeName = sizeName,
                Description = baseProduct.Description,
                StorageInstructions = baseProduct.StorageInstructions,
                Price = request.Price,
                CostPrice = request.CostPrice,
                StockQuantity = request.StockQuantity,
                ImageUrl = baseProduct.ImageUrl,
                Status = request.StockQuantity > 0 ? "stock" : "sold_out",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(newSize);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == newSize.CategoryId);
            return await MapToDtoAsync(newSize, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateSizesAsync(int id, UpdateProductSizesDto dto)
        {
            var baseProduct = await _productRepository.FindOneAsync(x => x.ProductId == id);
            if (baseProduct == null) return null;

            var requestedSizeNames = dto.Sizes
                .Select(s => s.SizeName.Trim().ToUpper())
                .ToList();

            if (requestedSizeNames.Count != requestedSizeNames.Distinct().Count())
                throw new InvalidOperationException("Danh sách size bị trùng tên, mỗi size phải có tên khác nhau.");

            var allProducts = await _productRepository.GetAllAsync();

            var sameFamily = allProducts
                .Where(p => p.Name.Equals(baseProduct.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var old in sameFamily)
                await _productRepository.DeleteAsync(x => x.ProductId == old.ProductId);

            var nextId = allProducts.Any() ? allProducts.Max(p => p.ProductId) + 1 : 1;

            Product? firstCreated = null;
            foreach (var sizeRequest in dto.Sizes)
            {
                var newSize = new Product
                {
                    ProductId = nextId++,
                    CategoryId = baseProduct.CategoryId,
                    Name = baseProduct.Name,
                    SizeName = sizeRequest.SizeName.Trim().ToUpper(),
                    Description = baseProduct.Description,
                    StorageInstructions = baseProduct.StorageInstructions,
                    Price = sizeRequest.Price,
                    CostPrice = sizeRequest.CostPrice,
                    StockQuantity = sizeRequest.StockQuantity,
                    ImageUrl = baseProduct.ImageUrl,
                    Status = sizeRequest.StockQuantity > 0 ? "stock" : "sold_out",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _productRepository.CreateAsync(newSize);
                firstCreated ??= newSize;
            }

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == baseProduct.CategoryId);
            return await MapToDtoAsync(firstCreated!, category?.Name ?? "Không xác định");
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