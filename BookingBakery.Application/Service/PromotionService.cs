using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Helper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BookingBakery.Application.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepo;
        private readonly IProductPromotionRepository _productPromotionRepo;
        private readonly IProductRepository _productRepo;
        private readonly HelperCloudinary _cloudinaryHelper;

        public PromotionService(
            IPromotionRepository promotionRepo,
            IProductPromotionRepository productPromotionRepo,
            IProductRepository productRepo,
            HelperCloudinary cloudinaryHelper)
        {
            _promotionRepo = promotionRepo;
            _productPromotionRepo = productPromotionRepo;
            _productRepo = productRepo;
            _cloudinaryHelper = cloudinaryHelper;
        }

        // ──────────────────────────────────────────────────────────────
        // 1. TẠO PROMOTION
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, PromotionResponse? Promotion)> CreatePromotionAsync(
            CreatePromotionRequest request)
        {
            if (request.EndDate <= request.StartDate)
                return (false, "Ngày kết thúc phải sau ngày bắt đầu.", null);

            if (request.BannerImage == null || request.BannerImage.Length == 0)
                return (false, "Vui lòng tải lên hình ảnh banner.", null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(request.BannerImage.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return (false,
                    "Định dạng file không hợp lệ. Chỉ chấp nhận: .jpg, .jpeg, .png, .gif, .webp", null);

            // Validate các product_id được gắn (nếu có)
            var validProductIds = new List<int>();
            var invalidProductIds = new List<int>();

            foreach (var productId in request.ProductIds.Distinct())
            {
                var product = await _productRepo.GetByIdAsync(productId);
                if (product == null)
                    invalidProductIds.Add(productId);
                else
                    validProductIds.Add(productId);
            }

            if (invalidProductIds.Count > 0)
                return (false,
                    $"Một số sản phẩm không tồn tại: {string.Join(", ", invalidProductIds.Select(id => $"#{id}"))}. " +
                    "Vui lòng kiểm tra lại danh sách sản phẩm.", null);

            // Upload banner lên Cloudinary
            string bannerUrl;
            try
            {
                using var stream = request.BannerImage.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.BannerImage.FileName, stream),
                    Folder = "promotions/banners"
                };

                var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                    return (false, $"Tải ảnh lên thất bại: {uploadResult.Error.Message}", null);

                bannerUrl = uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                return (false, $"Tải ảnh lên thất bại: {ex.Message}", null);
            }

            var promotionId = await _promotionRepo.GetNextPromotionIdAsync();
            var discountType = request.DiscountType == PromotionDiscountTypeOption.Percent
                ? PromotionDiscountType.Percent
                : PromotionDiscountType.Fixed;

            if (discountType == PromotionDiscountType.Percent &&
                (request.DiscountValue <= 0 || request.DiscountValue > 100))
                return (false, "Giá trị giảm theo % phải trong khoảng 1-100.", null);

            var now = DateTime.UtcNow;

            var promotion = new Promotion
            {
                PromotionId = promotionId,
                Title = request.Title.Trim(),
                Content = request.Content?.Trim(),
                BannerUrl = bannerUrl,
                DiscountType = discountType,
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = PromotionStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _promotionRepo.CreateAsync(promotion);

            foreach (var productId in validProductIds)
            {
                await _productPromotionRepo.CreateAsync(new ProductPromotion
                {
                    PromotionId = promotionId,
                    ProductId = productId,
                    CreatedAt = now
                });
            }

            var response = await BuildFullResponseAsync(promotion);
            return (true, $"Tạo chương trình khuyến mãi \"{promotion.Title}\" thành công.", response);
        }

        // ──────────────────────────────────────────────────────────────
        // 2. CẬP NHẬT THÔNG TIN PROMOTION (không gồm sản phẩm)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, PromotionResponse? Promotion)> UpdatePromotionAsync(
            int promotionId, UpdatePromotionRequest request)
        {
            var promotion = await _promotionRepo.GetByIdAsync(promotionId);
            if (promotion == null)
                return (false, "Không tìm thấy chương trình khuyến mãi.", null);

            if (!string.IsNullOrWhiteSpace(request.Title))
                promotion.Title = request.Title.Trim();

            if (request.Content != null)
                promotion.Content = request.Content.Trim();

            // Upload banner mới nếu có truyền lên
            if (request.BannerImage != null && request.BannerImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(request.BannerImage.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return (false,
                        "Định dạng file không hợp lệ. Chỉ chấp nhận: .jpg, .jpeg, .png, .gif, .webp", null);

                try
                {
                    using var stream = request.BannerImage.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(request.BannerImage.FileName, stream),
                        Folder = "promotions/banners"
                    };

                    var uploadResult = await _cloudinaryHelper.CloudinaryInstance.UploadAsync(uploadParams);
                    if (uploadResult.Error != null)
                        return (false, $"Tải ảnh lên thất bại: {uploadResult.Error.Message}", null);

                    promotion.BannerUrl = uploadResult.SecureUrl.ToString();
                }
                catch (Exception ex)
                {
                    return (false, $"Tải ảnh lên thất bại: {ex.Message}", null);
                }
            }

            if (request.DiscountType.HasValue)
                promotion.DiscountType = request.DiscountType.Value == PromotionDiscountTypeOption.Percent
                    ? PromotionDiscountType.Percent
                    : PromotionDiscountType.Fixed;

            if (request.DiscountValue.HasValue)
                promotion.DiscountValue = request.DiscountValue.Value;

            if (promotion.DiscountType == PromotionDiscountType.Percent &&
                (promotion.DiscountValue <= 0 || promotion.DiscountValue > 100))
                return (false, "Giá trị giảm theo % phải trong khoảng 1-100.", null);

            if (request.StartDate.HasValue)
                promotion.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                promotion.EndDate = request.EndDate.Value;

            if (promotion.EndDate <= promotion.StartDate)
                return (false, "Ngày kết thúc phải sau ngày bắt đầu.", null);

            await _promotionRepo.UpdateAsync(promotion);

            var response = await BuildFullResponseAsync(promotion);
            return (true, $"Cập nhật chương trình khuyến mãi \"{promotion.Title}\" thành công.", response);
        }

        // ──────────────────────────────────────────────────────────────
        // 3. XÓA PROMOTION
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> DeletePromotionAsync(int promotionId)
        {
            var promotion = await _promotionRepo.GetByIdAsync(promotionId);
            if (promotion == null)
                return (false, "Không tìm thấy chương trình khuyến mãi.");

            await _productPromotionRepo.DeleteByPromotionIdAsync(promotionId);
            await _promotionRepo.DeleteAsync(promotionId);

            return (true, $"Đã xóa chương trình khuyến mãi \"{promotion.Title}\" thành công.");
        }

        // ──────────────────────────────────────────────────────────────
        // 4. XEM TẤT CẢ (Admin/Staff)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<PromotionSummaryResponse>? Promotions)>
            GetAllPromotionsAsync()
        {
            var promotions = await _promotionRepo.GetAllAsync();
            var results = new List<PromotionSummaryResponse>();

            foreach (var p in promotions)
                results.Add(await BuildSummaryAsync(p));

            return (true, "Lấy danh sách chương trình khuyến mãi thành công.", results);
        }

        // ──────────────────────────────────────────────────────────────
        // 5. XEM PROMOTION ĐANG DIỄN RA (Public)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<PromotionSummaryResponse>? Promotions)>
            GetOngoingPromotionsAsync()
        {
            var promotions = await _promotionRepo.GetAllAsync();
            var now = DateTime.UtcNow;

            var ongoing = promotions
                .Where(p => p.Status == PromotionStatus.Active
                         && p.StartDate <= now
                         && p.EndDate >= now)
                .ToList();

            var results = new List<PromotionSummaryResponse>();
            foreach (var p in ongoing)
                results.Add(await BuildSummaryAsync(p));

            return (true, "Lấy danh sách chương trình khuyến mãi đang diễn ra thành công.", results);
        }

        // ──────────────────────────────────────────────────────────────
        // 6. XEM CHI TIẾT
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, PromotionResponse? Promotion)> GetPromotionByIdAsync(
            int promotionId)
        {
            var promotion = await _promotionRepo.GetByIdAsync(promotionId);
            if (promotion == null)
                return (false, "Không tìm thấy chương trình khuyến mãi.", null);

            var response = await BuildFullResponseAsync(promotion);
            return (true, "Lấy thông tin chương trình khuyến mãi thành công.", response);
        }

        // ──────────────────────────────────────────────────────────────
        // 7. THÊM SẢN PHẨM VÀO PROMOTION
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> AddProductsAsync(
            int promotionId, UpdatePromotionProductsRequest request)
        {
            var promotion = await _promotionRepo.GetByIdAsync(promotionId);
            if (promotion == null)
                return (false, "Không tìm thấy chương trình khuyến mãi.");

            var added = new List<int>();
            var alreadyExists = new List<int>();
            var invalidProducts = new List<int>();

            foreach (var productId in request.ProductIds.Distinct())
            {
                var product = await _productRepo.GetByIdAsync(productId);
                if (product == null)
                {
                    invalidProducts.Add(productId);
                    continue;
                }

                var exists = await _productPromotionRepo.ExistsAsync(promotionId, productId);
                if (exists)
                {
                    alreadyExists.Add(productId);
                    continue;
                }

                await _productPromotionRepo.CreateAsync(new ProductPromotion
                {
                    PromotionId = promotionId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });
                added.Add(productId);
            }

            if (invalidProducts.Count > 0)
                return (false,
                    $"Một số sản phẩm không tồn tại: {string.Join(", ", invalidProducts.Select(id => $"#{id}"))}.");

            var parts = new List<string>();
            if (added.Count > 0) parts.Add($"đã thêm {added.Count} sản phẩm");
            if (alreadyExists.Count > 0) parts.Add($"{alreadyExists.Count} sản phẩm đã có sẵn trong chương trình");

            return (true, $"Cập nhật thành công: {string.Join(", ", parts)}.");
        }

        // ──────────────────────────────────────────────────────────────
        // 8. GỠ SẢN PHẨM KHỎI PROMOTION
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> RemoveProductsAsync(
            int promotionId, UpdatePromotionProductsRequest request)
        {
            var promotion = await _promotionRepo.GetByIdAsync(promotionId);
            if (promotion == null)
                return (false, "Không tìm thấy chương trình khuyến mãi.");

            foreach (var productId in request.ProductIds.Distinct())
                await _productPromotionRepo.DeleteAsync(promotionId, productId);

            return (true, $"Đã gỡ {request.ProductIds.Distinct().Count()} sản phẩm khỏi chương trình khuyến mãi.");
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

        private async Task<PromotionResponse> BuildFullResponseAsync(Promotion p)
        {
            var links = await _productPromotionRepo.GetByPromotionIdAsync(p.PromotionId);
            var products = new List<PromotionProductItem>();
            var now = DateTime.UtcNow;
            var isOngoing = p.Status == PromotionStatus.Active && p.StartDate <= now && p.EndDate >= now;

            foreach (var link in links)
            {
                var product = await _productRepo.GetByIdAsync(link.ProductId);
                if (product == null) continue;

                var salePrice = isOngoing ? CalculateSalePrice(product.Price, p) : product.Price;

                products.Add(new PromotionProductItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    SalePrice = salePrice
                });
            }

            return new PromotionResponse
            {
                PromotionId = p.PromotionId,
                Title = p.Title,
                Content = p.Content,
                BannerUrl = p.BannerUrl,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                IsOngoing = isOngoing,
                Products = products,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        private async Task<PromotionSummaryResponse> BuildSummaryAsync(Promotion p)
        {
            var links = await _productPromotionRepo.GetByPromotionIdAsync(p.PromotionId);
            var now = DateTime.UtcNow;

            return new PromotionSummaryResponse
            {
                PromotionId = p.PromotionId,
                Title = p.Title,
                BannerUrl = p.BannerUrl,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                IsOngoing = p.Status == PromotionStatus.Active && p.StartDate <= now && p.EndDate >= now,
                ProductCount = links.Count
            };
        }

        /// <summary>
        /// Tính giá sau giảm theo Promotion. Dùng chung cho PromotionService và ProductService.
        /// </summary>
        public static decimal CalculateSalePrice(decimal originalPrice, Promotion promotion)
        {
            decimal salePrice;

            if (promotion.DiscountType == PromotionDiscountType.Percent)
                salePrice = originalPrice - (originalPrice * promotion.DiscountValue / 100);
            else
                salePrice = originalPrice - promotion.DiscountValue;

            return salePrice < 0 ? 0 : salePrice;
        }
    }
}