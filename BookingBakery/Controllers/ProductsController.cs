using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Xem toàn bộ sản phẩm trong kho (Khách vãng lai cũng xem được)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [EndpointSummary("Xem toàn bộ sản phẩm trong kho")]
        [EndpointDescription("Khách vãng lai cũng xem được")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Xem sản phẩm theo ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [EndpointSummary("Xem sản phẩm theo ID")]
        [EndpointDescription("Khách vãng lai cũng xem được")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}." });

            return Ok(product);
        }

        /// <summary>
        /// Thêm sản phẩm mới vào kho (Chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "1")]
        [EndpointSummary("Thêm sản phẩm mới vào kho")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.AddProductAsync(dto);
                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật giá sản phẩm (Chỉ Admin)
        /// </summary>
        [HttpPut("{id:int}/price")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật giá sản phẩm")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdateProductPriceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdatePriceAsync(id, dto.Price);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}." });

            return Ok(product);
        }

        /// <summary>
        /// Cập nhật mô tả sản phẩm (Chỉ Admin)
        /// </summary>
        [HttpPut("{id:int}/description")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật mô tả sản phẩm")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDescription(int id, [FromBody] UpdateProductDescriptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateDescriptionAsync(id, dto.Description);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}." });

            return Ok(product);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm (Admin và Staff)
        /// </summary>
        [HttpPut("{id:int}/stock")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Cập nhật số lượng sản phẩm")]
        [EndpointDescription("Admin và Staff")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateProductStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateStockAsync(id, dto.StockQuantity);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}." });

            return Ok(product);
        }

        /// <summary>
        /// Xóa sản phẩm khỏi kho (Chỉ Admin)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Xóa sản phẩm khỏi kho")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}." });

            return Ok(new { message = "Xóa sản phẩm thành công." });
        }

        /// <summary>
        /// Cập nhật giá sản phẩm theo tên (Chỉ Admin)
        /// </summary>
        [HttpPut("by-name/{name}/price")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật giá sản phẩm theo tên")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePriceByName(string name, [FromBody] UpdateProductPriceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdatePriceByNameAsync(name, dto.Price);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với tên = {name}." });

            return Ok(product);
        }

        /// <summary>
        /// Cập nhật mô tả sản phẩm theo tên (Chỉ Admin)
        /// </summary>
        [HttpPut("by-name/{name}/description")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật mô tả sản phẩm theo tên")]
        [EndpointDescription("Chỉ Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDescriptionByName(string name, [FromBody] UpdateProductDescriptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateDescriptionByNameAsync(name, dto.Description);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với tên = {name}." });

            return Ok(product);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm theo tên (Admin và Staff)
        /// </summary>
        [HttpPut("by-name/{name}/stock")]
        [Authorize(Roles = "1,2")]
        [EndpointSummary("Cập nhật số lượng sản phẩm theo tên")]
        [EndpointDescription("Admin và Staff")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStockByName(string name, [FromBody] UpdateProductStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateStockByNameAsync(name, dto.StockQuantity);
            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với tên = {name}." });

            return Ok(product);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên (Khách vãng lai cũng tìm được)
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [EndpointSummary("Tìm kiếm sản phẩm theo tên")]
        [EndpointDescription("Khách vãng lai cũng tìm được")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var products = await _productService.SearchProductsByNameAsync(name ?? string.Empty);
            return Ok(products);
        }
    }
}
