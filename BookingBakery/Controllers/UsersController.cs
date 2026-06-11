using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthRepository _userRepository;

        public UsersController(IAuthRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// Lấy danh sách tất cả người dùng (yêu cầu đăng nhập)
        /// <returns>Danh sách users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            var result = users.Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.Phone,
                u.RoleId,
                u.Status,
                u.CreatedAt,
                u.UpdatedAt
            });
            return Ok(result);
        }

        /// Lấy thông tin user theo ID (yêu cầu đăng nhập)
        /// <param name="id">User ID</param>
        /// <returns>Thông tin user</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"Không tìm thấy user với ID = {id}." });

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.Phone,
                user.RoleId,
                user.Status,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        /// <summary>
        /// Xóa user theo ID (yêu cầu đăng nhập)
        /// </summary>
        /// <param name="id">User ID</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"Không tìm thấy user với ID = {id}." });

            await _userRepository.DeleteAsync(u => u.UserId == id);
            return Ok(new { message = "Xóa user thành công." });
        }
    }
}
