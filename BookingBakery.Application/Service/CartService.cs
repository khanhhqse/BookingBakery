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
        private readonly IPromotionPriceHelper _promotionPriceHelper;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IPromotionPriceHelper promotionPriceHelper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _promotionPriceHelper = promotionPriceHelper; 
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

                // Sản phẩm đã bị gỡ khỏi hệ thống -> tự xóa khỏi giỏ + thông báo
                if (product == null)
                {
                    await _cartItemRepository.DeleteAsync(
                        ci => ci.CartId == item.CartId && ci.ProductId == item.ProductId);

                    removedNotices.Add(
                        "Rất tiếc, một sản phẩm trong giỏ hàng của bạn hiện không còn được bán nên đã được tự động xóa khỏi giỏ hàng.");
                    continue;
                }

                // BR-C03: sản phẩm đã hết hàng -> tự xóa khỏi giỏ + thông báo
                if (product.Status == "sold_out")
                {
                    await _cartItemRepository.DeleteAsync(
                        ci => ci.CartId == item.CartId && ci.ProductId == item.ProductId);

                    removedNotices.Add(
                        $"Rất tiếc, sản phẩm \"{product.Name}\" hiện đã hết hàng nên đã được tự động xóa khỏi giỏ hàng của bạn.");
                    continue;
                }

                var quantity = item.Quantity;

                // Tồn kho giảm xuống thấp hơn số đang có trong giỏ -> tự hạ về đúng số tồn kho + thông báo
                if (quantity > product.StockQuantity)
                {
                    quantity = product.StockQuantity;

                    item.Quantity = quantity;
                    await _cartItemRepository.UpdateAsync(
                        ci => ci.CartId == item.CartId && ci.ProductId == item.ProductId, item);

                    adjustedNotices.Add(
                        $"Sản phẩm \"{product.Name}\" hiện không còn đủ số lượng như trong giỏ hàng của bạn, chúng tôi đã điều chỉnh giảm số lượng giúp bạn, mời bạn kiểm tra lại giỏ hàng nhé.");
                }
                var (salePrice, hasPromotion, _) = await _promotionPriceHelper.GetSalePriceAsync(
                    product.ProductId, product.Price);

                itemDtos.Add(new CartItemDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    SalePrice = salePrice,
                    HasActivePromotion = hasPromotion,
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
                throw new InvalidOperationException(
                    "Sản phẩm bạn chọn hiện không tồn tại hoặc đã bị gỡ khỏi cửa hàng, vui lòng kiểm tra lại nhé.");

            if (product.Status == "sold_out")
                throw new InvalidOperationException(
                    $"Rất tiếc, sản phẩm \"{product.Name}\" hiện đã hết hàng, mời bạn chọn sản phẩm khác nhé.");

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == dto.ProductId);

            var currentQuantity = existingItem?.Quantity ?? 0;
            var newQuantity = currentQuantity + dto.Quantity;

            // BR-C02: số lượng tối đa mỗi sản phẩm trong giỏ là 50
            if (newQuantity > 50)
            {
                var remaining = 50 - currentQuantity;

                if (remaining <= 0)
                    throw new InvalidOperationException(
                        $"Giỏ hàng của bạn đã có tối đa 50 sản phẩm \"{product.Name}\", không thể thêm nữa.");

                throw new InvalidOperationException(
                    $"Mỗi đơn chỉ được đặt tối đa 50 sản phẩm \"{product.Name}\". Giỏ hàng của bạn hiện có {currentQuantity}, bạn có thể thêm tối đa {remaining} sản phẩm nữa nhé.");
            }

            // Không cho thêm vượt quá tồn kho thực tế - không lộ số tồn kho ra response
            if (newQuantity > product.StockQuantity)
                throw new InvalidOperationException(
                    $"Rất tiếc, sản phẩm \"{product.Name}\" hiện không đủ số lượng trong kho để thêm vào giỏ hàng. Mời bạn giảm số lượng hoặc chọn sản phẩm khác nhé.");

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
                throw new InvalidOperationException("Số lượng mỗi sản phẩm phải từ 1 đến 50 bạn nhé.");

            var product = await _productRepository.FindOneAsync(p => p.ProductId == productId);
            if (product == null)
                throw new InvalidOperationException(
                    "Sản phẩm bạn muốn cập nhật hiện không tồn tại hoặc đã bị gỡ khỏi cửa hàng, vui lòng kiểm tra lại nhé.");

            // Không lộ số tồn kho thực tế ra response
            if (quantity > product.StockQuantity)
                throw new InvalidOperationException(
                    $"Rất tiếc, sản phẩm \"{product.Name}\" hiện không đủ số lượng trong kho để cập nhật theo số lượng bạn yêu cầu.");

            var cart = await GetOrCreateCartAsync(userId);

            var item = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (item == null)
                throw new InvalidOperationException(
                    "Sản phẩm này hiện không có trong giỏ hàng của bạn, có thể đã bị xóa trước đó.");

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
                throw new InvalidOperationException(
                    "Sản phẩm này hiện không có trong giỏ hàng của bạn, có thể đã bị xóa trước đó.");

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

        public async Task<CartDto> RemoveItemsFromCartAsync(int userId, List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                throw new InvalidOperationException("Vui lòng chọn ít nhất một sản phẩm để xóa.");

            var cart = await GetOrCreateCartAsync(userId);

            // Kiểm tra từng productId có trong giỏ không
            var notFoundIds = new List<int>();
            foreach (var productId in productIds)
            {
                var item = await _cartItemRepository.FindOneAsync(
                    ci => ci.CartId == cart.CartId && ci.ProductId == productId);

                if (item == null)
                    notFoundIds.Add(productId);
            }

            if (notFoundIds.Count > 0)
                throw new InvalidOperationException(
                    $"Một số sản phẩm không có trong giỏ hàng của bạn: {string.Join(", ", notFoundIds.Select(id => $"#{id}"))}.");

            // Xóa tất cả các item được chọn
            await _cartItemRepository.DeleteManyAsync(
                ci => ci.CartId == cart.CartId && productIds.Contains(ci.ProductId));

            await TouchCartAsync(cart);
            return await GetCartByUserIdAsync(userId);
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