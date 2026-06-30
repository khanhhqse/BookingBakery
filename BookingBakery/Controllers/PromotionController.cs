using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Produces("application/json")]
    [Tags("Promotion")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpPost]
        [Authorize(Roles = "1,2")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo chương trình khuyến mãi mới")]
        [EndpointDescription("Admin và Staff. Dữ liệu gửi dưới dạng form-data, ảnh banner bắt buộc (key 'BannerImage'). ProductIds có thể để trống rồi gắn sau qua endpoint riêng. Discount type 1 là giảm theo %, 2 là tiền cố định")]
        [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] CreatePromotionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, promotion) = await _promotionService.CreatePromotionAsync(request);

            return success
                ? Ok(new { message, data = promotion })
                : BadRequest(new { message });
        }

        [HttpPut("{promotionId:int}")]
        [Authorize(Roles = "1,2")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật thông tin chương trình khuyến mãi")]
        [EndpointDescription("Admin và Staff. Dữ liệu gửi dưới dạng form-data. Để trống BannerImage nếu không muốn đổi ảnh. Dùng endpoint riêng để thêm/gỡ sản phẩm.")]
        [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            [FromRoute] int promotionId,
            [FromForm] UpdatePromotionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, promotion) = await _promotionService.UpdatePromotionAsync(promotionId, request);

            return success
                ? Ok(new { message, data = promotion })
                : BadRequest(new { message });
        }

        [HttpDelete("{promotionId:int}")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Xóa chương trình khuyến mãi")]
        [EndpointDescription("Admin và Staff. Xóa luôn các liên kết sản phẩm.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] int promotionId)
        {
            var (success, message) = await _promotionService.DeletePromotionAsync(promotionId);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Xem tất cả chương trình khuyến mãi")]
        [EndpointDescription("Admin và Staff. Bao gồm cả chương trình đã kết thúc hoặc bị vô hiệu hóa.")]
        [ProducesResponseType(typeof(List<PromotionSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var (success, message, promotions) = await _promotionService.GetAllPromotionsAsync();
            return Ok(new { message, data = promotions });
        }

        [HttpGet("ongoing")]
        [AllowAnonymous]
        [EndpointSummary("Xem chương trình khuyến mãi đang diễn ra")]
        [EndpointDescription("Khách vãng lai cũng xem được. Chỉ trả về promotion đang active và trong khoảng start_date - end_date.")]
        [ProducesResponseType(typeof(List<PromotionSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOngoing()
        {
            var (success, message, promotions) = await _promotionService.GetOngoingPromotionsAsync();
            return Ok(new { message, data = promotions });
        }

        [HttpGet("{promotionId:int}")]
        [AllowAnonymous]
        [EndpointSummary("Xem chi tiết chương trình khuyến mãi")]
        [EndpointDescription("Khách vãng lai cũng xem được. Trả về đầy đủ danh sách sản phẩm kèm theo.")]
        [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int promotionId)
        {
            var (success, message, promotion) = await _promotionService.GetPromotionByIdAsync(promotionId);

            return success
                ? Ok(new { message, data = promotion })
                : NotFound(new { message });
        }

        [HttpPost("{promotionId:int}/products")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Thêm sản phẩm vào chương trình khuyến mãi")]
        [EndpointDescription("Admin và Staff. Truyền danh sách ProductIds cần thêm.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProducts(
            [FromRoute] int promotionId,
            [FromBody] UpdatePromotionProductsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _promotionService.AddProductsAsync(promotionId, request);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
        }

        [HttpDelete("{promotionId:int}/products")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Gỡ sản phẩm khỏi chương trình khuyến mãi")]
        [EndpointDescription("Admin và Staff. Truyền danh sách ProductIds cần gỡ.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveProducts(
            [FromRoute] int promotionId,
            [FromBody] UpdatePromotionProductsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _promotionService.RemoveProductsAsync(promotionId, request);

            return success
                ? Ok(new { message })
                : BadRequest(new { message });
        }
    }
}