using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IUserProfileService
    {
        Task<UserProfileResponseDto?> GetByUserIdAsync(int userId);
        Task<UserProfileResponseDto> UpsertMyProfileAsync(int userId, UpdateUserProfileDto dto);

        Task<IEnumerable<UserProfileResponseDto>> GetAllAsync();
        Task<UserProfileResponseDto?> GetByProfileIdAsync(int profileId);
        Task<IEnumerable<UserProfileResponseDto>> GetByUserIdsAsync(IEnumerable<int> userIds);
        Task<(bool Success, string Message, List<CustomerSearchResponse>? Results)> SearchCustomersAsync(
    CustomerSearchRequest request);
    }
}