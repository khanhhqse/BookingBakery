using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        private int GetCurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (sub == null || !int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Không xác định được người dùng.");
            return userId;
        }

        // ── ADMIN: BR-V01 ────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "1")]
        [EndpointSummary("Lấy danh sách tất cả voucher")]
        public async Task<IActionResult> GetAll()
            => Ok(await _voucherService.GetAllAsync());

        [HttpGet("{id}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Lấy chi tiết 1 voucher")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _voucherService.GetByIdAsync(id));

        [HttpGet("search")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Tìm voucher theo mã code")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
            => Ok(await _voucherService.SearchByCodeAsync(keyword));

        [HttpGet("filter")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Lọc voucher theo khoảng ngày")]
        public async Task<IActionResult> Filter([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
            => Ok(await _voucherService.FilterByDateRangeAsync(from, to));

        [HttpPost]
        [Authorize(Roles = "1")]
        [EndpointSummary("Thêm mới voucher")]
        [EndpointDescription("Admin tạo voucher mới.ApplyScope = 0 => Áp dụng cho tất cả sp, ApplyScope = 1 => áp dụng cho 1 số sp => bắt buộc phải truyền ProductIds (danh sách sản phẩm được áp dụng). Mã Code phải là duy nhất trong hệ thống.")]
        public async Task<IActionResult> Create([FromBody] CreateVoucherRequest request)
            => Ok(await _voucherService.CreateAsync(request));

        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Chỉnh sửa thông tin voucher")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVoucherRequest request)
            => Ok(await _voucherService.UpdateAsync(id, request));

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Vô hiệu hóa voucher")]
        public async Task<IActionResult> Delete(int id)
        {
            await _voucherService.DeleteAsync(id);
            return NoContent();
        }

        // ── CUSTOMER ─────────────────────────────────────

        [HttpGet("me/used")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem voucher đã sử dụng")]
        public async Task<IActionResult> GetMyUsedVouchers()
            => Ok(await _voucherService.GetMyUsedVouchersAsync(GetCurrentUserId()));

        [HttpGet("me/unused")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem voucher chưa sử dụng")]
        public async Task<IActionResult> GetMyUnusedVouchers()
            => Ok(await _voucherService.GetMyUnusedVouchersAsync(GetCurrentUserId()));

        [HttpPost("apply")]
        [Authorize(Roles = "3")]
        [EndpointSummary("Xem trước hiệu lực + số tiền được giảm khi áp voucher vào giỏ hàng (checkout)")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
            => Ok(await _voucherService.ValidateAndCalculateAsync(GetCurrentUserId(), request.VoucherCode));
    }
}