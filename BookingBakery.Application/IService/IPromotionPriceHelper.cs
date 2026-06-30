namespace BookingBakery.Application.IService
{
    /// <summary>
    /// Helper tính giá sau khuyến mãi cho 1 sản phẩm — dùng chung bởi
    /// ProductService, CartService, OrderService để tránh lặp logic.
    /// </summary>
    public interface IPromotionPriceHelper
    {
        /// <summary>
        /// Trả về (giá sau giảm, có đang khuyến mãi không).
        /// Nếu có nhiều Promotion active cùng lúc, chọn giá thấp nhất (BR đã thống nhất).
        /// </summary>
        Task<(decimal SalePrice, bool HasActivePromotion, string? PromotionTitle)> GetSalePriceAsync(
            int productId, decimal originalPrice);
    }
}