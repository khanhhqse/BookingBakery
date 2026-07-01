using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,2")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Xem toàn bộ danh sách danh mục (Mọi người đều có quyền xem kể cả khách chưa đăng nhập)
        /// </summary>
        /// <returns>Danh sách các danh mục</returns>
        [HttpGet]
        [AllowAnonymous]
        [EndpointSummary("Xem toàn bộ danh sách danh mục")]
        [EndpointDescription("Mọi người đều có quyền xem kể cả khách chưa đăng nhập")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Xem thông tin danh mục theo ID (Chỉ Admin và Staff)
        /// </summary>
        [HttpGet("{id:int}")]
        [EndpointSummary("Xem thông tin danh mục theo ID")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(category);
        }

        /// <summary>
        /// Thêm danh mục mới (Chỉ Admin và Staff)
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Thêm danh mục mới")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif, .webp" });
            }

            try
            {
                var category = await _categoryService.AddCategoryAsync(dto);
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Sửa thông tin danh mục (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("{id:int}")]
        [EndpointSummary("Sửa thông tin danh mục")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, dto);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(category);
        }

        /// <summary>
        /// Xóa danh mục (Chỉ Admin và Staff)
        /// </summary>
        [HttpDelete("{id:int}")]
        [EndpointSummary("Xóa danh mục")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(new { message = "Xóa danh mục thành công." });
        }

        /// <summary>
        /// Xóa danh mục theo tên (Chỉ Admin và Staff)
        /// </summary>
        [HttpDelete("by-name/{name}")]
        [EndpointSummary("Xóa danh mục theo tên")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var success = await _categoryService.DeleteCategoryByNameAsync(name);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy danh mục với tên = {name}." });

            return Ok(new { message = "Xóa danh mục theo tên thành công." });
        }

        /// <summary>
        /// Sửa thông tin danh mục theo tên (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("by-name/{name}")]
        [EndpointSummary("Sửa thông tin danh mục theo tên")]
        [EndpointDescription("Chỉ Admin và Staff")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateByName(string name, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var category = await _categoryService.UpdateCategoryByNameAsync(name, dto);
                if (category == null)
                    return NotFound(new { message = $"Không tìm thấy danh mục với tên = {name}." });

                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật hình ảnh danh mục theo ID (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật hình ảnh danh mục")]
        [EndpointDescription("Chỉ Admin và Staff, file upload qua form-data với key 'file'")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif, .webp" });

            try
            {
                using var stream = file.OpenReadStream();
                var updatedCategory = await _categoryService.UpdateCategoryImageAsync(id, stream, file.FileName);
                
                if (updatedCategory == null)
                    return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

                return Ok(updatedCategory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật hình ảnh danh mục theo tên (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("by-name/{name}/image")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật hình ảnh danh mục theo tên")]
        [EndpointDescription("Chỉ Admin và Staff, file upload qua form-data với key 'file'")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImageByName(string name, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif, .webp" });

            try
            {
                using var stream = file.OpenReadStream();
                var updatedCategory = await _categoryService.UpdateCategoryImageByNameAsync(name, stream, file.FileName);
                
                if (updatedCategory == null)
                    return NotFound(new { message = $"Không tìm thấy danh mục với tên = {name}." });

                return Ok(updatedCategory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
