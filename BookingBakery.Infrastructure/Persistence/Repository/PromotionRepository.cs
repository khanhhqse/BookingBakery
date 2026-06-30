using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly IMongoCollection<Promotion> _promotions;

        public PromotionRepository(MongoDbContext context)
        {
            _promotions = context.GetCollection<Promotion>("promotions");

            var idx = Builders<Promotion>.IndexKeys.Ascending(p => p.PromotionId);
            _promotions.Indexes.CreateOne(
                new CreateIndexModel<Promotion>(idx,
                    new CreateIndexOptions { Unique = true, Name = "idx_promotion_id" }));
        }

        public async Task<List<Promotion>> GetAllAsync()
        {
            return await _promotions.Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int promotionId)
        {
            return await _promotions.Find(p => p.PromotionId == promotionId).FirstOrDefaultAsync();
        }

        public async Task<int> GetNextPromotionIdAsync()
        {
            var last = await _promotions.Find(_ => true)
                .SortByDescending(p => p.PromotionId)
                .Limit(1)
                .FirstOrDefaultAsync();
            return (last?.PromotionId ?? 0) + 1;
        }

        public async Task CreateAsync(Promotion promotion)
        {
            await _promotions.InsertOneAsync(promotion);
        }

        public async Task UpdateAsync(Promotion promotion)
        {
            promotion.UpdatedAt = DateTime.UtcNow;
            await _promotions.ReplaceOneAsync(p => p.PromotionId == promotion.PromotionId, promotion);
        }

        public async Task DeleteAsync(int promotionId)
        {
            await _promotions.DeleteOneAsync(p => p.PromotionId == promotionId);
        }
    }
}