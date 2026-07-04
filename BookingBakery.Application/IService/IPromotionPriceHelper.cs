namespace BookingBakery.Application.IService
{
    /// <summary>Helper tính giá sau khuyến mãi cho 1 sản phẩm.</summary>
    public interface IPromotionPriceHelper
    {
        /// <summary>
        /// Tính sale price cho 1 product.
        /// Nếu có nhiều Promotion active → chọn giá thấp nhất.
        /// </summary>
        Task<(decimal SalePrice, bool HasPromotion, string? PromotionTitle)> GetSalePriceAsync(
            int productId, decimal originalPrice);
    }
}