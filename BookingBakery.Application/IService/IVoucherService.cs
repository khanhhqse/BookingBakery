using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IVoucherService
    {
        // Admin (BR-V01)
        Task<List<VoucherDto>> GetAllAsync();
        Task<VoucherDto> GetByIdAsync(int voucherId);
        Task<VoucherDto> CreateAsync(CreateVoucherRequest request);
        Task<VoucherDto> UpdateAsync(int voucherId, UpdateVoucherRequest request);
        Task DeleteAsync(int voucherId); // vô hiệu hóa
        Task<List<VoucherDto>> SearchByCodeAsync(string keyword);
        Task<List<VoucherDto>> FilterByDateRangeAsync(DateOnly? from, DateOnly? to);

        // Customer
        Task<List<VoucherDto>> GetMyUsedVouchersAsync(int userId);
        Task<List<VoucherDto>> GetMyUnusedVouchersAsync(int userId);

        /// Preview: tính toán, validate BR-V02/03/04 nhưng KHÔNG đánh dấu đã dùng
        Task<ApplyVoucherResultDto> ValidateAndCalculateAsync(int userId, string voucherCode);

        /// Gọi khi Order được tạo thành công — đánh dấu voucher đã được dùng
        Task ConfirmVoucherUsageAsync(int userId, string voucherCode);
    }
}