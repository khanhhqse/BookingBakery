using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class ProductIngredientRepository : IProductIngredientRepository
    {
        private readonly IMongoCollection<ProductIngredient> _collection;

        public ProductIngredientRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<ProductIngredient>("productingredients");

            // Tạo unique compound index cho cặp (product_id, ingredient_id) làm composite key
            var indexKeys = Builders<ProductIngredient>.IndexKeys
                .Ascending(pi => pi.ProductId)
                .Ascending(pi => pi.IngredientId);

            var indexModel = new CreateIndexModel<ProductIngredient>(
                indexKeys, new CreateIndexOptions { Unique = true });

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<ProductIngredient>> FindManyAsync(Expression<Func<ProductIngredient, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<ProductIngredient?> FindOneAsync(Expression<Func<ProductIngredient, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ProductIngredient entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<ProductIngredient, bool>> filter, ProductIngredient entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<ProductIngredient, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
