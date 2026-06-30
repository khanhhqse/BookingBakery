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

        public async Task<(decimal SalePrice, bool HasActivePromotion, string? PromotionTitle)> GetSalePriceAsync(
            int productId, decimal originalPrice)
        {
            var links = await _productPromotionRepo.GetByProductIdAsync(productId);
            if (links.Count == 0)
                return (originalPrice, false, null);

            var now = DateTime.UtcNow;
            decimal? bestSalePrice = null;
            string? bestTitle = null;

            foreach (var link in links)
            {
                var promotion = await _promotionRepo.GetByIdAsync(link.PromotionId);
                if (promotion == null) continue;

                var isOngoing = promotion.Status == PromotionStatus.Active
                             && promotion.StartDate <= now
                             && promotion.EndDate >= now;

                if (!isOngoing) continue;

                var salePrice = CalculateSalePrice(originalPrice, promotion);

                // Nếu có nhiều promotion active cùng lúc → chọn giá thấp nhất
                if (bestSalePrice == null || salePrice < bestSalePrice)
                {
                    bestSalePrice = salePrice;
                    bestTitle = promotion.Title;
                }
            }

            return bestSalePrice.HasValue
                ? (bestSalePrice.Value, true, bestTitle)
                : (originalPrice, false, null);
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