using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IVoucherRepository
    {
        Task<List<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(int voucherId);
        Task<Voucher?> GetByCodeAsync(string code);
        Task<List<Voucher>> SearchByCodeAsync(string keyword);
        Task<List<Voucher>> FilterByDateRangeAsync(DateOnly? from, DateOnly? to);
        Task CreateAsync(Voucher voucher);
        Task UpdateAsync(int voucherId, Voucher voucher);
        Task DeleteAsync(int voucherId); // soft-delete: set status = inactive
    }
}