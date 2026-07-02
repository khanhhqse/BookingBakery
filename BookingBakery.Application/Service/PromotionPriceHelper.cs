using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class PromotionPriceHelper : IPromotionPriceHelper
    {
        private readonly IProductPromotionRepository _productPromotionRepo;
        private readonly IPromotionRepository _promotionRepo;

        public PromotionPriceHelper(
            IProductPromotionRepository productPromotionRepo,
            IPromotionRepository promotionRepo)
        {
            _productPromotionRepo = productPromotionRepo;
            _promotionRepo = promotionRepo;
        }

        public async Task<Dictionary<string, (decimal SalePrice, bool HasPromotion)>> GetSalePricesAsync(
            int productId, List<(string Name, decimal Price)> sizes)
        {
            var result = sizes.ToDictionary(
                s => s.Name,
                s => (SalePrice: s.Price, HasPromotion: false));

            var links = await _productPromotionRepo.GetByProductIdAsync(productId);
            if (links.Count == 0) return result;

            var now = DateTime.UtcNow;

            foreach (var link in links)
            {
                var promotion = await _promotionRepo.GetByIdAsync(link.PromotionId);
                if (promotion == null) continue;

                var isOngoing = promotion.Status == PromotionStatus.Active
                             && promotion.StartDate <= now
                             && promotion.EndDate >= now;
                if (!isOngoing) continue;

                // Xác định size nào được áp — empty = tất cả
                var targetSizes = link.ApplicableSizes == null || link.ApplicableSizes.Count == 0
                    ? sizes.Select(s => s.Name).ToList()
                    : link.ApplicableSizes;

                foreach (var sizeName in targetSizes)
                {
                    var sizeInfo = sizes.FirstOrDefault(s =>
                        s.Name.Equals(sizeName, StringComparison.OrdinalIgnoreCase));

                    if (sizeInfo.Name == null) continue;

                    var salePrice = CalculateSalePrice(sizeInfo.Price, promotion);

                    // Chọn giá thấp nhất nếu nhiều promotion active
                    if (!result.ContainsKey(sizeInfo.Name) || salePrice < result[sizeInfo.Name].SalePrice)
                        result[sizeInfo.Name] = (salePrice, true);
                }
            }

            return result;
        }

        public async Task<(decimal SalePrice, bool HasPromotion)> GetSalePriceForSizeAsync(
            int productId, string sizeName, decimal originalPrice)
        {
            var links = await _productPromotionRepo.GetByProductIdAsync(productId);
            if (links.Count == 0) return (originalPrice, false);

            var now = DateTime.UtcNow;
            decimal? bestSalePrice = null;

            foreach (var link in links)
            {
                var promotion = await _promotionRepo.GetByIdAsync(link.PromotionId);
                if (promotion == null) continue;

                var isOngoing = promotion.Status == PromotionStatus.Active
                             && promotion.StartDate <= now
                             && promotion.EndDate >= now;
                if (!isOngoing) continue;

                // Kiểm tra size có được áp không
                var appliesToSize = link.ApplicableSizes == null
                                 || link.ApplicableSizes.Count == 0
                                 || link.ApplicableSizes.Any(s =>
                                        s.Equals(sizeName, StringComparison.OrdinalIgnoreCase));

                if (!appliesToSize) continue;

                var salePrice = CalculateSalePrice(originalPrice, promotion);
                if (bestSalePrice == null || salePrice < bestSalePrice)
                    bestSalePrice = salePrice;
            }

            return bestSalePrice.HasValue
                ? (bestSalePrice.Value, true)
                : (originalPrice, false);
        }

        public static decimal CalculateSalePrice(decimal originalPrice, Promotion promotion)
        {
            decimal salePrice = promotion.DiscountType == PromotionDiscountType.Percent
                ? originalPrice - (originalPrice * promotion.DiscountValue / 100)
                : originalPrice - promotion.DiscountValue;

            return salePrice < 0 ? 0 : salePrice;
        }
    }
}