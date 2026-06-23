using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    [Produces("application/json")]
    [Tags("Order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "3")]
        [EndpointSummary("Đặt hàng từ giỏ hàng hiện tại")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var (success, message, order) = await _orderService.PlaceOrderAsync(userId.Value, request);

            return success
                ? Ok(new { message, data = order })
                : BadRequest(new { message });
        }

        [HttpGet("me")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem danh sách đơn hàng của bản thân")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var (success, message, orders) = await _orderService.GetMyOrdersAsync(userId.Value);
            return Ok(new { message, data = orders });
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Xem toàn bộ đơn hàng (FIFO — Admin/Staff)")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (success, message, orders) = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(new { message, data = orders });
        }

        [HttpGet("{orderId:int}")]
        [Authorize(Roles = "1,2,3")]
        [EndpointSummary("Xem chi tiết một đơn hàng")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderDetail([FromRoute] int orderId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userId == null || userRole == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var (success, message, order) = await _orderService.GetOrderDetailAsync(orderId, userId.Value, userRole);

            return success
                ? Ok(new { message, data = order })
                : NotFound(new { message });
        }

        [HttpPut("{orderId:int}/status")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Cập nhật trạng thái đơn hàng (Staff/Admin)")]
        [EndpointDescription("Giá trị newStatus: 1 = Đang làm (tự động trừ stock), 2 = Đang giao, 3 = Hoàn thành. Có thể bỏ qua bước giữa (VD: Chờ xác nhận → Đang giao) nhưng không được kéo ngược trạng thái (BR-L01).")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromRoute] int orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng." });

            var (success, message) = await _orderService.UpdateOrderStatusAsync(orderId, request, userId.Value);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
        }

        [HttpPut("{orderId:int}/cancel")]
        [Authorize(Roles = "1,2,3")]
        [EndpointSummary("Hủy đơn hàng")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelOrder(
            [FromRoute] int orderId,
            [FromBody] CancelOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userId == null || userRole == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var (success, message) = await _orderService.CancelOrderAsync(orderId, request, userId.Value, userRole);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
        }

        [HttpPut("{orderId:int}/confirm-received")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xác nhận đã nhận hàng (Customer)")]
        [EndpointDescription("Customer xác nhận đã nhận hàng khi đơn đang ở trạng thái 'Đang giao'. Đơn sẽ chuyển sang 'Hoàn thành'. Nếu sau 48h giao mà không xác nhận, hệ thống sẽ tự động hoàn thành (BR-L03).")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmReceived([FromRoute] int orderId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được thông tin người dùng. Vui lòng đăng nhập lại." });

            var (success, message) = await _orderService.CustomerConfirmReceivedAsync(orderId, userId.Value);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
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

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}