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
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("me")]
        [EndpointSummary("Xem giỏ hàng của tôi")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            return Ok(cart);
        }

        [HttpPost("items")]
        [EndpointSummary("Thêm sản phẩm vào giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var cart = await _cartService.AddItemToCartAsync(userId.Value, dto);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("items/{productId:int}")]
        [EndpointSummary("Cập nhật số lượng sản phẩm trong giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemQuantity(int productId, [FromBody] UpdateCartItemQuantityDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var cart = await _cartService.UpdateItemQuantityAsync(userId.Value, productId, dto.Quantity);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("items/{productId:int}")]
        [EndpointSummary("Xóa sản phẩm khỏi giỏ hàng")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var cart = await _cartService.RemoveItemFromCartAsync(userId.Value, productId);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [EndpointSummary("Xóa toàn bộ giỏ hàng")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            await _cartService.ClearCartAsync(userId.Value);
            return Ok(new { message = "Đã xóa toàn bộ giỏ hàng." });
        }

        [HttpDelete("items")]
        [EndpointSummary("Xóa nhiều sản phẩm khỏi giỏ hàng")]
        [EndpointDescription("Truyền danh sách productId vào body để xóa nhiều sản phẩm cùng lúc. VD: [1,2] -> productId 1 và 2 sẽ bị xóa")]
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

        private int? GetCurrentUserId()
        {
            // AuthService phát token với claim "sub" = user.UserId.
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }
}