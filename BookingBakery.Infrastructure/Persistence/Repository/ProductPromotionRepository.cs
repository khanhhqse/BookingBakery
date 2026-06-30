using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public class ProductPromotionRepository : IProductPromotionRepository
    {
        private readonly IMongoCollection<ProductPromotion> _collection;

        public ProductPromotionRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<ProductPromotion>("product_promotions");

            // Unique compound — 1 sản phẩm không bị gán trùng 1 promotion
            var idx = Builders<ProductPromotion>.IndexKeys
                .Ascending(pp => pp.PromotionId)
                .Ascending(pp => pp.ProductId);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<ProductPromotion>(idx,
                    new CreateIndexOptions { Unique = true, Name = "idx_promotion_product_unique" }));
        }

        public async Task<List<ProductPromotion>> GetByPromotionIdAsync(int promotionId)
        {
            return await _collection.Find(pp => pp.PromotionId == promotionId).ToListAsync();
        }

        public async Task<List<ProductPromotion>> GetByProductIdAsync(int productId)
        {
            return await _collection.Find(pp => pp.ProductId == productId).ToListAsync();
        }

        public async Task<bool> ExistsAsync(int promotionId, int productId)
        {
            return await _collection
                .Find(pp => pp.PromotionId == promotionId && pp.ProductId == productId)
                .AnyAsync();
        }

        public async Task CreateAsync(ProductPromotion entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task DeleteAsync(int promotionId, int productId)
        {
            await _collection.DeleteOneAsync(pp => pp.PromotionId == promotionId && pp.ProductId == productId);
        }

        public async Task DeleteByPromotionIdAsync(int promotionId)
        {
            await _collection.DeleteManyAsync(pp => pp.PromotionId == promotionId);
        }
    }
}