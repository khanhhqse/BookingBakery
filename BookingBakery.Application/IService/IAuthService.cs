using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto, int roleId);
        Task<LoginResultDto?> LoginAsync(LoginDto dto);
        Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(int roleId);
    }
}
