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
        private readonly IProductPromotionRepository _productPromotionRepo;
        private readonly IPromotionRepository _promotionRepo;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            HelperCloudinary cloudinaryHelper,
            IPromotionPriceHelper promotionPriceHelper,
            IProductPromotionRepository productPromotionRepo,
            IPromotionRepository promotionRepo)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cloudinaryHelper = cloudinaryHelper;
            _promotionPriceHelper = promotionPriceHelper;
            _productPromotionRepo = productPromotionRepo;
            _promotionRepo = promotionRepo;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var result = new List<ProductDto>();
            foreach (var p in products)
            {
                var categoryName = categoryMap.TryGetValue(p.CategoryId, out var name) ? name : "Không xác định";
                result.Add(await MapToDtoAsync(p, categoryName));
            }
            return result;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            if (p == null) return null;

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";
            return await MapToDtoAsync(p, categoryName);
        }

        public async Task<ProductDto> AddProductAsync(CreateProductDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                throw new ArgumentException("Hình ảnh sản phẩm là bắt buộc.");

            // Không bắt buộc size — có thể thêm sau qua POST /api/Products/{id}/sizes

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
                Description = dto.Description,
                StorageInstructions = dto.StorageInstructions?.Trim(),
                Price = 0,
                CostPrice = dto.CostPrice,
                StockQuantity = dto.StockQuantity,
                ImageUrl = uploadResult.SecureUrl.ToString(),
                Status = "stock",
                Sizes = string.IsNullOrWhiteSpace(dto.SizeName)
                    ? new List<ProductSize>()
                    : new List<ProductSize>
                    {
                        new ProductSize
                        {
                            Name  = dto.SizeName.Trim(),
                            Price = dto.Price
                        }
                    },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(product);
            return await MapToDtoAsync(product, category.Name);
        }

        public async Task<ProductDto?> UpdateStockAsync(int id, int quantity)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            product.StockQuantity = quantity;
            product.Status = quantity > 0 ? "stock" : "sold_out";
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdatePriceAsync(int id, decimal price)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            product.Price = price;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateDescriptionAsync(int id, string? description)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            product.Description = description;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateStorageInstructionsAsync(int id, string? storageInstructions)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            product.StorageInstructions = storageInstructions?.Trim();
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateSizesAsync(int id, UpdateProductSizesDto dto)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            // Validate không trùng tên size
            var sizeNames = dto.Sizes.Select(s => s.Name.Trim().ToUpper()).ToList();
            if (sizeNames.Distinct().Count() != sizeNames.Count)
                throw new InvalidOperationException("Danh sách size có tên bị trùng. Vui lòng kiểm tra lại.");

            product.Sizes = dto.Sizes.Select(s => new ProductSize
            {
                Name = s.Name.Trim(),
                Price = s.Price
            }).ToList();
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> AddSizeAsync(int id, ProductSizeRequest request)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            // Validate không trùng tên size
            var exists = product.Sizes.Any(
                s => s.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
                throw new InvalidOperationException(
                    $"Size \"{request.Name}\" đã tồn tại trong sản phẩm này. " +
                    "Vui lòng dùng tên size khác hoặc cập nhật size hiện có.");

            product.Sizes.Add(new ProductSize
            {
                Name = request.Name.Trim(),
                Price = request.Price
            });
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateStockByNameAsync(string name, int quantity)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null) return null;

            product.StockQuantity = quantity;
            product.Status = quantity > 0 ? "stock" : "sold_out";
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdatePriceByNameAsync(string name, decimal price)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null) return null;

            product.Price = price;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateDescriptionByNameAsync(string name, string? description)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null) return null;

            product.Description = description;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsByNameAsync(string name)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var result = new List<ProductDto>();
            foreach (var p in products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
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
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return false;

            await _productRepository.DeleteAsync(p => p.ProductId == id);
            return true;
        }

        public async Task<ProductDto?> UpdateImageAsync(int id, Stream imageStream, string fileName)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateImageByNameAsync(string name, Stream imageStream, string fileName)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };
            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên thất bại: {uploadResult.Error.Message}");

            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            return await MapToDtoAsync(product, category?.Name ?? "Không xác định");
        }

        public async Task<ProductDto?> UpdateNameAndCategoryAsync(int id, UpdateProductNameAndCategoryDto dto)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null) return null;

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {dto.CategoryId} không tồn tại.");

            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            return await MapToDtoAsync(product, category.Name);
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPER
        // ──────────────────────────────────────────────────────────────

        private async Task<string?> GetActivePromotionTitleAsync(int productId)
        {
            var links = await _productPromotionRepo.GetByProductIdAsync(productId);
            var now = DateTime.UtcNow;

            foreach (var link in links)
            {
                var promotion = await _promotionRepo.GetByIdAsync(link.PromotionId);
                if (promotion == null) continue;

                if (promotion.Status == PromotionStatus.Active
                    && promotion.StartDate <= now
                    && promotion.EndDate >= now)
                    return promotion.Title;
            }
            return null;
        }

        private async Task<ProductDto> MapToDtoAsync(Product p, string categoryName)
        {
            // Tính sale price cho từng size bằng GetSalePricesAsync
            var sizeInputs = p.Sizes.Select(s => (s.Name, s.Price)).ToList();
            var salePriceMap = await _promotionPriceHelper.GetSalePricesAsync(p.ProductId, sizeInputs);

            var sizeDtos = p.Sizes.Select(s =>
            {
                var hasSaleInfo = salePriceMap.TryGetValue(s.Name, out var info);
                var salePrice = hasSaleInfo ? info.SalePrice : s.Price;
                var hasPromotion = hasSaleInfo && info.HasPromotion;

                return new ProductSizeDto
                {
                    Name = s.Name,
                    Price = s.Price,
                    SalePrice = salePrice,
                    HasPromotion = hasPromotion
                };
            }).ToList();

            var hasAnyPromotion = sizeDtos.Any(s => s.HasPromotion);
            var activePromotionTitle = salePriceMap.Values
                .FirstOrDefault(v => v.HasPromotion).HasPromotion
                    ? await GetActivePromotionTitleAsync(p.ProductId)
                    : null;

            return new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = categoryName,
                Name = p.Name,
                Description = p.Description,
                StorageInstructions = p.StorageInstructions,
                CostPrice = p.CostPrice,
                HasActivePromotion = hasAnyPromotion,
                ActivePromotionTitle = activePromotionTitle,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                Sizes = sizeDtos,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}