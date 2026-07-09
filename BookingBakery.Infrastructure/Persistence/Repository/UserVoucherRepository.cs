using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public class UserVoucherRepository : IUserVoucherRepository
    {
        private readonly IMongoCollection<UserVoucher> _collection;

        public UserVoucherRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<UserVoucher>("user_vouchers");

            var idx = Builders<UserVoucher>.IndexKeys
                .Ascending(uv => uv.VoucherId)
                .Ascending(uv => uv.UserId);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<UserVoucher>(idx,
                    new CreateIndexOptions { Unique = true, Name = "idx_voucher_user_unique" }));
        }

        public async Task<List<UserVoucher>> GetByUserIdAsync(int userId, string? status = null)
        {
            var fb = Builders<UserVoucher>.Filter;
            var filter = fb.Eq(uv => uv.UserId, userId);
            if (!string.IsNullOrEmpty(status))
                filter &= fb.Eq(uv => uv.Status, status);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<UserVoucher?> GetByVoucherAndUserAsync(int voucherId, int userId)
            => await _collection
                .Find(uv => uv.VoucherId == voucherId && uv.UserId == userId)
                .FirstOrDefaultAsync();

        public async Task CreateAsync(UserVoucher entity)
            => await _collection.InsertOneAsync(entity);

        public async Task MarkUsedAsync(int voucherId, int userId)
        {
            var update = Builders<UserVoucher>.Update
                .Set(uv => uv.Status, "used")
                .Set(uv => uv.UsedAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(
                uv => uv.VoucherId == voucherId && uv.UserId == userId, update);
        }
    }
}