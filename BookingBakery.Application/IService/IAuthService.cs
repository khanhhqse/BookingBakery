using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto, int roleId);
        Task<LoginResultDto?> LoginAsync(LoginDto dto);
        Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(int roleId);
        /// <summary>Đổi mật khẩu — yêu cầu nhập đúng mật khẩu cũ trước khi đổi.</summary>
        Task<(bool Success, string Message)> ChangePasswordAsync(
            int userId, ChangePasswordDto dto);
    }
}
