using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    [Tags("Cart")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>Xem giỏ hàng của bản thân</summary>
        [HttpGet("me")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem giỏ hàng của bản thân")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Thêm sản phẩm vào giỏ hàng</summary>
        [HttpPost("items")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Thêm sản phẩm vào giỏ hàng")]
        [EndpointDescription("Bắt buộc chọn size. Cùng sản phẩm nhưng khác size = 2 dòng riêng biệt trong giỏ.")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            try
            {
                var cart = await _cartService.AddToCartAsync(userId.Value, dto);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Cập nhật số lượng sản phẩm trong giỏ</summary>
        [HttpPut("items/{productId:int}")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Cập nhật số lượng sản phẩm trong giỏ")]
        [EndpointDescription("Cần truyền thêm sizeName vì cùng productId có thể có nhiều size khác nhau trong giỏ.")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemQuantity(
            [FromRoute] int productId,
            [FromQuery] string sizeName,
            [FromBody] UpdateCartItemQuantityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(sizeName))
                return BadRequest(new { message = "Vui lòng cung cấp tên size." });

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            try
            {
                var cart = await _cartService.UpdateCartItemQuantityAsync(userId.Value, productId, sizeName, dto);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Xóa 1 sản phẩm (theo productId + size) khỏi giỏ hàng</summary>
        [HttpDelete("items/{productId:int}")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa 1 sản phẩm khỏi giỏ hàng")]
        [EndpointDescription("Cần truyền sizeName để xác định đúng item cần xóa.")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItem(
            [FromRoute] int productId,
            [FromQuery] string sizeName)
        {
            if (string.IsNullOrWhiteSpace(sizeName))
                return BadRequest(new { message = "Vui lòng cung cấp tên size." });

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            try
            {
                var cart = await _cartService.RemoveCartItemAsync(userId.Value, productId, sizeName);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Xóa nhiều sản phẩm khỏi giỏ hàng</summary>
        [HttpDelete("items")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa nhiều sản phẩm khỏi giỏ hàng")]
        [EndpointDescription("Truyền danh sách productId vào body. Sẽ xóa tất cả size của các productId đó.")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItems([FromBody] List<int> productIds)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            try
            {
                var cart = await _cartService.RemoveItemsFromCartAsync(userId.Value, productIds);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Xóa toàn bộ giỏ hàng</summary>
        [HttpDelete("me")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa toàn bộ giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var cart = await _cartService.ClearCartAsync(userId.Value);
            return Ok(cart);
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}