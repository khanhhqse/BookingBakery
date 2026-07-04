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

        [HttpGet("me")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem giỏ hàng của bản thân")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            return Ok(cart);
        }

        [HttpPost("items")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Thêm sản phẩm vào giỏ hàng")]
        [EndpointDescription("Mỗi ProductId đã là 1 size riêng biệt — không cần chọn size thêm.")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

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

        [HttpPut("items/{productId:int}")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Cập nhật số lượng sản phẩm trong giỏ")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemQuantity(
            [FromRoute] int productId,
            [FromBody] UpdateCartItemQuantityDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var cart = await _cartService.UpdateCartItemQuantityAsync(userId.Value, productId, dto);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("items/{productId:int}")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa 1 sản phẩm khỏi giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveItem([FromRoute] int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.RemoveCartItemAsync(userId.Value, productId);
            return Ok(cart);
        }

        [HttpDelete("items")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa nhiều sản phẩm khỏi giỏ hàng")]
        [EndpointDescription("Truyền danh sách productId vào body. Ví dụ: [1, 2, 3]")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItems([FromBody] List<int> productIds)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

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

        [HttpDelete("me")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xóa toàn bộ giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.ClearCartAsync(userId.Value);
            return Ok(cart);
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}