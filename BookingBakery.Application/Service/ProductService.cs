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

        public ProductService(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            HelperCloudinary cloudinaryHelper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            return products.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = categoryMap.TryGetValue(p.CategoryId, out var name) ? name : "Không xác định",
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            if (p == null)
                return null;

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == p.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = categoryName,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        public async Task<ProductDto> AddProductAsync(CreateProductDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                throw new ArgumentException("Hình ảnh sản phẩm là bắt buộc.");

            // Kiểm tra category_id có tồn tại không
            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {dto.CategoryId} không tồn tại.");

            // Kiểm tra trùng tên sản phẩm
            var existingProduct = await _productRepository.FindOneAsync(p => p.Name.ToLower() == dto.Name.ToLower());
            if (existingProduct != null)
                throw new InvalidOperationException($"Sản phẩm với tên '{dto.Name}' đã tồn tại.");

            using var imageStream = dto.Image.OpenReadStream();
            string fileName = dto.Image.FileName;

            // Upload lên Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };

            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            string finalImageUrl = uploadResult.SecureUrl.ToString();

            var all = await _productRepository.GetAllAsync();
            var nextId = all.Any() ? all.Max(p => p.ProductId) + 1 : 1;

            var product = new Product
            {
                ProductId = nextId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                StockQuantity = dto.StockQuantity,
                ImageUrl = finalImageUrl,
                Status = "stock",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(product);

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdateStockAsync(int id, int quantity)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null)
                return null;

            product.StockQuantity = quantity;
            // Tự động cập nhật status khi tồn kho thay đổi
            product.Status = quantity > 0 ? "stock" : "sold_out";
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdatePriceAsync(int id, decimal price)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null)
                return null;

            product.Price = price;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdateDescriptionAsync(int id, string? description)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null)
                return null;

            product.Description = description;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null)
                return false;

            await _productRepository.DeleteAsync(p => p.ProductId == id);
            return true;
        }

        public async Task<ProductDto?> UpdateStockByNameAsync(string name, int quantity)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null)
                return null;

            product.StockQuantity = quantity;
            // Tự động cập nhật status khi tồn kho thay đổi
            product.Status = quantity > 0 ? "stock" : "sold_out";
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdatePriceByNameAsync(string name, decimal price)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null)
                return null;

            product.Price = price;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdateDescriptionByNameAsync(string name, string? description)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null)
                return null;

            product.Description = description;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsByNameAsync(string name)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var filteredProducts = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            return filteredProducts.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = categoryMap.TryGetValue(p.CategoryId, out var catName) ? catName : "Không xác định",
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == categoryId);
            if (category == null)
                throw new InvalidOperationException($"Danh mục với ID = {categoryId} không tồn tại.");

            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p => p.CategoryId == categoryId);

            return filteredProducts.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = category.Name,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<ProductDto?> UpdateImageAsync(int id, Stream imageStream, string fileName)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == id);
            if (product == null)
                return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };

            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == id, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto?> UpdateImageByNameAsync(string name, Stream imageStream, string fileName)
        {
            var product = await _productRepository.FindOneAsync(p => p.Name.ToLower() == name.ToLower());
            if (product == null)
                return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "products/images"
            };

            var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(p => p.ProductId == product.ProductId, product);

            var category = await _categoryRepository.FindOneAsync(c => c.CategoryId == product.CategoryId);
            var categoryName = category?.Name ?? "Không xác định";

            return new ProductDto
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
