using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
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

            // Drop index cũ nếu còn
            try { _collection.Indexes.DropOne("cart_id_1_product_id_1_size_name_1"); } catch { }
            try { _collection.Indexes.DropOne("idx_cart_product_size_unique"); } catch { }
            try { _collection.Indexes.DropOne("cart_id_1_product_id_1"); } catch { }

            // Composite key mới: (cart_id, product_id) — size đã nằm trong Product
            var idx = Builders<CartItem>.IndexKeys
                .Ascending(ci => ci.CartId)
                .Ascending(ci => ci.ProductId);

            _collection.Indexes.CreateOne(
                new CreateIndexModel<CartItem>(idx,
                    new CreateIndexOptions { Unique = true, Name = "idx_cart_product_unique" }));
        }

        public async Task<IEnumerable<CartItem>> FindManyAsync(Expression<Func<CartItem, bool>> filter)
            => await _collection.Find(filter).ToListAsync();

        public async Task<CartItem?> FindOneAsync(Expression<Func<CartItem, bool>> filter)
            => await _collection.Find(filter).FirstOrDefaultAsync();

        public async Task CreateAsync(CartItem entity)
            => await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(Expression<Func<CartItem, bool>> filter, CartItem entity)
            => await _collection.ReplaceOneAsync(filter, entity);

        public async Task DeleteAsync(Expression<Func<CartItem, bool>> filter)
            => await _collection.DeleteOneAsync(filter);

        public async Task DeleteManyAsync(Expression<Func<CartItem, bool>> filter)
            => await _collection.DeleteManyAsync(filter);
    }
}