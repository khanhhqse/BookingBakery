using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IPromotionRepository
    {
        Task<List<Promotion>> GetAllAsync();
        Task<Promotion?> GetByIdAsync(int promotionId);
        Task<int> GetNextPromotionIdAsync();
        Task CreateAsync(Promotion promotion);
        Task UpdateAsync(Promotion promotion);
        Task DeleteAsync(int promotionId);
    }
}