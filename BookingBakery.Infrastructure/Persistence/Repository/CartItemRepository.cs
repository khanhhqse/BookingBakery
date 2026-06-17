using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly IMongoCollection<CartItem> _collection;

        public CartItemRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<CartItem>("cartitems");

            // Composite key (cart_id, product_id): unique compound index
            var indexKeys = Builders<CartItem>.IndexKeys
                .Ascending(ci => ci.CartId)
                .Ascending(ci => ci.ProductId);

            var indexModel = new CreateIndexModel<CartItem>(
                indexKeys, new CreateIndexOptions { Unique = true });

            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<CartItem>> FindManyAsync(Expression<Func<CartItem, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<CartItem?> FindOneAsync(Expression<Func<CartItem, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CartItem entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<CartItem, bool>> filter, CartItem entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<CartItem, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }

        public async Task DeleteManyAsync(Expression<Func<CartItem, bool>> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }
    }
}