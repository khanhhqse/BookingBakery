using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface ICartRepository
    {
        Task<IEnumerable<Cart>> GetAllAsync();
        Task<Cart?> FindOneAsync(Expression<Func<Cart, bool>> filter);
        Task CreateAsync(Cart entity);
        Task UpdateAsync(Expression<Func<Cart, bool>> filter, Cart entity);
        Task DeleteAsync(Expression<Func<Cart, bool>> filter);
    }
}