using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<CartDto> GetCartByUserIdAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var items = await _cartItemRepository.FindManyAsync(ci => ci.CartId == cart.CartId);

            var itemDtos = new List<CartItemDto>();
            var removedNotices = new List<string>();
            var adjustedNotices = new List<string>();

            foreach (var item in items)
            {
                var product = await _productRepository.FindOneAsync(p => p.ProductId == item.ProductId);

                // BR-C03: sản phẩm không còn tồn tại hoặc đã hết hàng -> tự xóa khỏi giỏ + thông báo
                if (product == null || product.Status == "sold_out")
                {
                    await _cartItemRepository.DeleteAsync(
                        ci => ci.CartId == item.CartId && ci.ProductId == item.ProductId);

                    removedNotices.Add(
                        $"Sản phẩm \"{product?.Name ?? $"#{item.ProductId}"}\" đã hết hàng và bị xóa khỏi giỏ hàng của bạn.");
                    continue;
                }

                var quantity = item.Quantity;

                // Tồn kho giảm xuống thấp hơn số đang có trong giỏ -> tự hạ về đúng số tồn kho + thông báo
                if (quantity > product.StockQuantity)
                {
                    var oldQuantity = quantity;
                    quantity = product.StockQuantity;

                    item.Quantity = quantity;
                    await _cartItemRepository.UpdateAsync(
                        ci => ci.CartId == item.CartId && ci.ProductId == item.ProductId, item);

                    adjustedNotices.Add(
                        $"Sản phẩm \"{product.Name}\" chỉ còn {product.StockQuantity} trong kho, số lượng trong giỏ đã giảm từ {oldQuantity} xuống {quantity}.");
                }

                itemDtos.Add(new CartItemDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            return new CartDto
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = itemDtos,
                RemovedItemNotices = removedNotices,
                AdjustedItemNotices = adjustedNotices,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }

        public async Task<CartDto> AddItemToCartAsync(int userId, AddToCartDto dto)
        {
            var product = await _productRepository.FindOneAsync(p => p.ProductId == dto.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm với ID = {dto.ProductId} không tồn tại.");

            if (product.Status == "sold_out")
                throw new InvalidOperationException($"Sản phẩm \"{product.Name}\" hiện đã hết hàng.");

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == dto.ProductId);

            var currentQuantity = existingItem?.Quantity ?? 0;
            var newQuantity = currentQuantity + dto.Quantity;

            // BR-C02: số lượng tối đa mỗi sản phẩm trong giỏ là 50
            if (newQuantity > 50)
                throw new InvalidOperationException(
                    $"Số lượng sản phẩm \"{product.Name}\" trong giỏ không được vượt quá 50 (hiện tại {currentQuantity}, thêm {dto.Quantity}).");

            // Không cho thêm vượt quá số lượng tồn kho thực tế
            if (newQuantity > product.StockQuantity)
                throw new InvalidOperationException(
                    $"Sản phẩm \"{product.Name}\" chỉ còn {product.StockQuantity} trong kho (hiện tại trong giỏ: {currentQuantity}, không thể thêm {dto.Quantity}).");

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
                await _cartItemRepository.UpdateAsync(
                    ci => ci.CartId == cart.CartId && ci.ProductId == dto.ProductId, existingItem);
            }
            else
            {
                await _cartItemRepository.CreateAsync(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await TouchCartAsync(cart);
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity < 1 || quantity > 50)
                throw new InvalidOperationException("Số lượng phải từ 1 đến 50.");

            var product = await _productRepository.FindOneAsync(p => p.ProductId == productId);
            if (product == null)
                throw new InvalidOperationException($"Sản phẩm với ID = {productId} không tồn tại.");

            if (quantity > product.StockQuantity)
                throw new InvalidOperationException(
                    $"Sản phẩm \"{product.Name}\" chỉ còn {product.StockQuantity} trong kho.");

            var cart = await GetOrCreateCartAsync(userId);

            var item = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (item == null)
                throw new InvalidOperationException("Sản phẩm này không có trong giỏ hàng của bạn.");

            item.Quantity = quantity;
            await _cartItemRepository.UpdateAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == productId, item);

            await TouchCartAsync(cart);
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDto> RemoveItemFromCartAsync(int userId, int productId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var item = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (item == null)
                throw new InvalidOperationException("Sản phẩm này không có trong giỏ hàng của bạn.");

            await _cartItemRepository.DeleteAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            await TouchCartAsync(cart);
            return await GetCartByUserIdAsync(userId);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            await _cartItemRepository.DeleteManyAsync(ci => ci.CartId == cart.CartId);
            await TouchCartAsync(cart);
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _cartRepository.FindOneAsync(c => c.UserId == userId);
            if (cart != null)
                return cart;

            var all = await _cartRepository.GetAllAsync();
            var nextCartId = all.Any() ? all.Max(c => c.CartId) + 1 : 1;

            var newCart = new Cart
            {
                CartId = nextCartId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cartRepository.CreateAsync(newCart);
            return newCart;
        }

        private async Task TouchCartAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(c => c.CartId == cart.CartId, cart);
        }
    }
}