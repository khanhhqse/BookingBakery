using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly IMongoCollection<UserProfile> _collection;

        public UserProfileRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<UserProfile>("user_profiles");
        }

        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<UserProfile?> GetByIdAsync(int id, string idField = "profile_id")
        {
            var filter = Builders<UserProfile>.Filter.Eq(idField, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<UserProfile?> FindOneAsync(Expression<Func<UserProfile, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(UserProfile entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<UserProfile, bool>> filter, UserProfile entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<UserProfile, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}