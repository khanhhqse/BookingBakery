using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingBakery.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _profileService;
        private readonly IAuthService _authService;

        public UserProfilesController(IUserProfileService profileService, IAuthService authService)
        {
            _profileService = profileService;
            _authService = authService;
        }

        // Lấy thông tin profile của chính mình. 
        [HttpGet("me")]
        [EndpointSummary("Hiển thị profile của account đã được tạo trước đó ")]
        [EndpointDescription("Nếu 1 vài thông tin bị null thì dùng method PUT để chỉnh sửa thêm thông tin ")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _profileService.GetByUserIdAsync(userId);

            if (profile == null)
                return NotFound("Bạn chưa có profile. Gọi PUT /api/UserProfiles/me để tạo mới.");

            return Ok(profile);
        }

        // Cập nhật profile (tự tạo mới nếu chưa có)
        [HttpPut("me")]
        [EndpointSummary("Chỉnh sửa profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var profile = await _profileService.UpsertMyProfileAsync(userId, dto);
            return Ok(profile);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            return userId;
        }


        // Admin: xem tất cả profile trong hệ thống
        [HttpGet]
        [Authorize(Roles = "1")] // ⚠️ Đổi "1" thành đúng RoleId của Admin
        [EndpointSummary("Admin lấy tất cả profile")]
        public async Task<IActionResult> GetAllProfiles()
        {
            var profiles = await _profileService.GetAllAsync();
            return Ok(profiles);
        }

        // Admin: xem profile theo profileId
        [HttpGet("{profileId:int}")]
        [Authorize(Roles = "1")] // ⚠️ Đổi "1" thành đúng RoleId của Admin
        [EndpointSummary("Admin lấy profile by profile id ")]
        public async Task<IActionResult> GetProfileById(int profileId)
        {
            var profile = await _profileService.GetByProfileIdAsync(profileId);

            if (profile == null)
                return NotFound("Không tìm thấy profile.");

            return Ok(profile);
        }

        // Admin: xem profile theo userId
        [HttpGet("by-user/{userId:int}")]
        [Authorize(Roles = "1")] // ⚠️ Đổi "1" thành đúng RoleId của Admin
        [EndpointSummary("Admin lấy profile by user id")]
        public async Task<IActionResult> GetProfileByUserId(int userId)
        {
            var profile = await _profileService.GetByUserIdAsync(userId);

            if (profile == null)
                return NotFound($"Không tìm thấy profile cho userId = {userId}.");

            return Ok(profile);
        }

        // Admin: lấy danh sách tất cả Staff
        [HttpGet("staff")]
        [Authorize(Roles = "1")] // Admin
        [EndpointSummary("Admin lấy profile tất cả Staff")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _authService.GetUsersByRoleAsync(RoleIds.Staff);
            return Ok(staff);
        }

        // Admin: lấy profile của tất cả Customer
        [HttpGet("customers")]
        [Authorize(Roles = "1")] // Admin
        [EndpointSummary("Admin lấy profile của tất cả Customer")]
        public async Task<IActionResult> GetAllCustomerProfiles()
        {
            var customers = await _authService.GetUsersByRoleAsync(RoleIds.Customer);
            var customerUserIds = customers.Select(u => u.UserId);
            var profiles = await _profileService.GetByUserIdsAsync(customerUserIds);
            return Ok(profiles);
        }
    }
}