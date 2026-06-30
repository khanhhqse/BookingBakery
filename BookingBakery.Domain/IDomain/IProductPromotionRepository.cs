using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IProductPromotionRepository
    {
        Task<List<ProductPromotion>> GetByPromotionIdAsync(int promotionId);
        Task<List<ProductPromotion>> GetByProductIdAsync(int productId);
        Task<bool> ExistsAsync(int promotionId, int productId);
        Task CreateAsync(ProductPromotion entity);
        Task DeleteAsync(int promotionId, int productId);
        Task DeleteByPromotionIdAsync(int promotionId);
    }
}