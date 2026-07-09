using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookingBakery.Infrastructure.Persistence
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly IMongoCollection<Voucher> _collection;

        public VoucherRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Voucher>("vouchers");

            var codeIdx = Builders<Voucher>.IndexKeys.Ascending(v => v.Code);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<Voucher>(codeIdx,
                    new CreateIndexOptions { Unique = true, Name = "idx_voucher_code_unique" }));

            var idIdx = Builders<Voucher>.IndexKeys.Ascending(v => v.VoucherId);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<Voucher>(idIdx,
                    new CreateIndexOptions { Unique = true, Name = "idx_voucher_id_unique" }));
        }

        public async Task<List<Voucher>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<Voucher?> GetByIdAsync(int voucherId)
            => await _collection.Find(v => v.VoucherId == voucherId).FirstOrDefaultAsync();

        public async Task<Voucher?> GetByCodeAsync(string code)
            => await _collection.Find(v => v.Code == code).FirstOrDefaultAsync();

        public async Task<List<Voucher>> SearchByCodeAsync(string keyword)
        {
            var filter = Builders<Voucher>.Filter.Regex(
                v => v.Code, new BsonRegularExpression(keyword, "i"));
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Voucher>> FilterByDateRangeAsync(DateOnly? from, DateOnly? to)
        {
            var fb = Builders<Voucher>.Filter;
            var filter = fb.Empty;

            if (from.HasValue)
                filter &= fb.Gte(v => v.EndDate, from.Value);
            if (to.HasValue)
                filter &= fb.Lte(v => v.StartDate, to.Value);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task CreateAsync(Voucher voucher)
            => await _collection.InsertOneAsync(voucher);

        public async Task UpdateAsync(int voucherId, Voucher voucher)
            => await _collection.ReplaceOneAsync(v => v.VoucherId == voucherId, voucher);

        public async Task DeleteAsync(int voucherId)
        {
            var update = Builders<Voucher>.Update
                .Set(v => v.Status, "inactive")
                .Set(v => v.UpdatedAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(v => v.VoucherId == voucherId, update);
        }
    }
}