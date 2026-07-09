using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IVoucherProductRepository
    {
        Task<List<VoucherProduct>> GetByVoucherIdAsync(int voucherId);
        Task<bool> ExistsAsync(int voucherId, int productId);
        Task CreateAsync(VoucherProduct entity);
        Task DeleteAsync(int voucherId, int productId);
        Task DeleteByVoucherIdAsync(int voucherId);
    }
}