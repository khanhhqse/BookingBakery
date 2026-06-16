using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id, string idField = "product_id");
        Task<Product?> FindOneAsync(Expression<Func<Product, bool>> filter);
        Task CreateAsync(Product entity);
        Task UpdateAsync(Expression<Func<Product, bool>> filter, Product entity);
        Task DeleteAsync(Expression<Func<Product, bool>> filter);
    }
}
