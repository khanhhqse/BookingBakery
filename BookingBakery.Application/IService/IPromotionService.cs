using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IPromotionService
    {
        Task<(bool Success, string Message, PromotionResponse? Promotion)> CreatePromotionAsync(
            CreatePromotionRequest request);

        Task<(bool Success, string Message, PromotionResponse? Promotion)> UpdatePromotionAsync(
            int promotionId, UpdatePromotionRequest request);

        Task<(bool Success, string Message)> DeletePromotionAsync(int promotionId);

        Task<(bool Success, string Message, List<PromotionSummaryResponse>? Promotions)> GetAllPromotionsAsync();

        /// <summary>Khách vãng lai cũng xem được — chỉ trả promotion đang active + đang diễn ra.</summary>
        Task<(bool Success, string Message, List<PromotionSummaryResponse>? Promotions)> GetOngoingPromotionsAsync();

        Task<(bool Success, string Message, PromotionResponse? Promotion)> GetPromotionByIdAsync(int promotionId);

        /// <summary>Thêm sản phẩm vào chương trình khuyến mãi.</summary>
        Task<(bool Success, string Message)> AddProductsAsync(
            int promotionId, UpdatePromotionProductsRequest request);

        /// <summary>Gỡ sản phẩm khỏi chương trình khuyến mãi.</summary>
        Task<(bool Success, string Message)> RemoveProductsAsync(
            int promotionId, UpdatePromotionProductsRequest request);
    }
}