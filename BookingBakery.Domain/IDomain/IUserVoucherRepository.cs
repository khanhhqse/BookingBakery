using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IUserVoucherRepository
    {
        Task<List<UserVoucher>> GetByUserIdAsync(int userId, string? status = null);
        Task<UserVoucher?> GetByVoucherAndUserAsync(int voucherId, int userId);
        Task CreateAsync(UserVoucher entity);
        Task MarkUsedAsync(int voucherId, int userId);
    }
}