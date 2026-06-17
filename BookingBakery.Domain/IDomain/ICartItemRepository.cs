using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface ICartItemRepository
    {
        Task<IEnumerable<CartItem>> FindManyAsync(Expression<Func<CartItem, bool>> filter);
        Task<CartItem?> FindOneAsync(Expression<Func<CartItem, bool>> filter);
        Task CreateAsync(CartItem entity);
        Task UpdateAsync(Expression<Func<CartItem, bool>> filter, CartItem entity);
        Task DeleteAsync(Expression<Func<CartItem, bool>> filter);
        Task DeleteManyAsync(Expression<Func<CartItem, bool>> filter);
    }
}