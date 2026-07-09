using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public class VoucherProductRepository : IVoucherProductRepository
    {
        private readonly IMongoCollection<VoucherProduct> _collection;

        public VoucherProductRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<VoucherProduct>("voucher_products");

            var idx = Builders<VoucherProduct>.IndexKeys
                .Ascending(vp => vp.VoucherId)
                .Ascending(vp => vp.ProductId);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<VoucherProduct>(idx,
                    new CreateIndexOptions { Unique = true, Name = "idx_voucher_product_unique" }));
        }

        public async Task<List<VoucherProduct>> GetByVoucherIdAsync(int voucherId)
            => await _collection.Find(vp => vp.VoucherId == voucherId).ToListAsync();

        public async Task<bool> ExistsAsync(int voucherId, int productId)
            => await _collection
                .Find(vp => vp.VoucherId == voucherId && vp.ProductId == productId)
                .AnyAsync();

        public async Task CreateAsync(VoucherProduct entity)
            => await _collection.InsertOneAsync(entity);

        public async Task DeleteAsync(int voucherId, int productId)
            => await _collection.DeleteOneAsync(
                vp => vp.VoucherId == voucherId && vp.ProductId == productId);

        public async Task DeleteByVoucherIdAsync(int voucherId)
            => await _collection.DeleteManyAsync(vp => vp.VoucherId == voucherId);
    }
}