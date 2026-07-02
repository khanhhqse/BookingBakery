namespace BookingBakery.Application.IService
{
    /// <summary>
    /// Helper tính giá sau khuyến mãi theo từng size — dùng chung bởi
    /// ProductService, CartService, OrderService.
    /// </summary>
    public interface IPromotionPriceHelper
    {
        /// <summary>
        /// Tính sale price cho TẤT CẢ size của 1 sản phẩm.
        /// Trả về dict: sizeName → (salePrice, hasPromotion).
        /// Nếu có nhiều Promotion active cùng lúc → chọn giá thấp nhất.
        /// </summary>
        Task<Dictionary<string, (decimal SalePrice, bool HasPromotion)>> GetSalePricesAsync(
            int productId, List<(string Name, decimal Price)> sizes);

        /// <summary>
        /// Tính sale price cho 1 size cụ thể — dùng khi thêm vào giỏ hàng.
        /// </summary>
        Task<(decimal SalePrice, bool HasPromotion)> GetSalePriceForSizeAsync(
            int productId, string sizeName, decimal originalPrice);
    }
}