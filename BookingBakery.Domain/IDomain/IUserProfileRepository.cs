using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface IUserProfileRepository
    {
        Task<IEnumerable<UserProfile>> GetAllAsync();
        Task<UserProfile?> GetByIdAsync(int id, string idField = "profile_id");
        Task<UserProfile?> FindOneAsync(Expression<Func<UserProfile, bool>> filter);

        /// <summary>Tìm nhiều profile theo điều kiện — dùng cho search/filter.</summary>
        Task<IEnumerable<UserProfile>> FindManyAsync(Expression<Func<UserProfile, bool>> filter);

        Task CreateAsync(UserProfile entity);
        Task UpdateAsync(Expression<Func<UserProfile, bool>> filter, UserProfile entity);
        Task DeleteAsync(Expression<Func<UserProfile, bool>> filter);
    }
}