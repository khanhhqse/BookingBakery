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
            return await BuildCartDtoAsync(cart);
        }

        public async Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
                throw new InvalidOperationException($"Sản phẩm #{dto.ProductId} không tồn tại.");

            if (product.Status == "sold_out")
                throw new InvalidOperationException($"Rất tiếc, sản phẩm \"{product.Name}\" đã hết hàng.");

            if (product.Sizes == null || product.Sizes.Count == 0)
                throw new InvalidOperationException($"Sản phẩm \"{product.Name}\" chưa có size. Vui lòng liên hệ nhân viên.");

            var selectedSize = product.Sizes.FirstOrDefault(
                s => s.Name.Equals(dto.SizeName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (selectedSize == null)
                throw new InvalidOperationException(
                    $"Size \"{dto.SizeName}\" không tồn tại cho sản phẩm \"{product.Name}\". " +
                    $"Các size hợp lệ: {string.Join(", ", product.Sizes.Select(s => s.Name))}.");

            // Tính sale price theo size
            var (salePrice, _) = await _promotionPriceHelper.GetSalePriceForSizeAsync(
                product.ProductId, selectedSize.Name, selectedSize.Price);

            var existingItem = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId
                   && ci.ProductId == dto.ProductId
                   && ci.SizeName == selectedSize.Name);

            if (existingItem != null)
            {
                var newQty = existingItem.Quantity + dto.Quantity;

                if (newQty > 50)
                    throw new InvalidOperationException(
                        $"Số lượng \"{product.Name}\" ({selectedSize.Name}) trong giỏ không được vượt quá 50.");

                if (newQty > product.StockQuantity)
                    throw new InvalidOperationException(
                        $"Rất tiếc, \"{product.Name}\" ({selectedSize.Name}) không đủ hàng để thêm.");

                existingItem.Quantity = newQty;
                existingItem.SizePrice = salePrice;
                await _cartItemRepository.UpdateAsync(
                    ci => ci.CartId == cart.CartId
                       && ci.ProductId == dto.ProductId
                       && ci.SizeName == selectedSize.Name,
                    existingItem);
            }
            else
            {
                if (dto.Quantity > product.StockQuantity)
                    throw new InvalidOperationException(
                        $"Rất tiếc, \"{product.Name}\" ({selectedSize.Name}) không đủ hàng.");

                await _cartItemRepository.CreateAsync(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = dto.ProductId,
                    SizeName = selectedSize.Name,
                    SizePrice = salePrice,
                    Quantity = dto.Quantity
                });
            }

            await TouchCartAsync(cart);
            return await BuildCartDtoAsync(cart);
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(
            int userId, int productId, string sizeName, UpdateCartItemQuantityDto dto)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var item = await _cartItemRepository.FindOneAsync(
                ci => ci.CartId == cart.CartId
                   && ci.ProductId == productId
                   && ci.SizeName == sizeName.Trim());

            if (item == null)
                throw new InvalidOperationException(
                    $"Không tìm thấy sản phẩm #{productId} size \"{sizeName}\" trong giỏ hàng.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null && dto.Quantity > product.StockQuantity)
                throw new InvalidOperationException("Rất tiếc, số lượng yêu cầu vượt quá tồn kho hiện có.");

            item.Quantity = dto.Quantity;
            await _cartItemRepository.UpdateAsync(
                ci => ci.CartId == cart.CartId
                   && ci.ProductId == productId
                   && ci.SizeName == sizeName.Trim(),
                item);

            await TouchCartAsync(cart);
            return await BuildCartDtoAsync(cart);
        }

        public async Task<CartDto> RemoveCartItemAsync(int userId, int productId, string sizeName)
        {
            var cart = await GetOrCreateCartAsync(userId);

            await _cartItemRepository.DeleteAsync(
                ci => ci.CartId == cart.CartId
                   && ci.ProductId == productId
                   && ci.SizeName == sizeName.Trim());

            await TouchCartAsync(cart);
            return await BuildCartDtoAsync(cart);
        }

        public async Task<CartDto> RemoveItemsFromCartAsync(int userId, List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                throw new InvalidOperationException("Vui lòng chọn ít nhất một sản phẩm để xóa.");

            var cart = await GetOrCreateCartAsync(userId);

            await _cartItemRepository.DeleteManyAsync(
                ci => ci.CartId == cart.CartId && productIds.Contains(ci.ProductId));

            await TouchCartAsync(cart);
            return await BuildCartDtoAsync(cart);
        }

        public async Task<CartDto> ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            await _cartItemRepository.DeleteManyAsync(ci => ci.CartId == cart.CartId);
            await TouchCartAsync(cart);
            return await BuildCartDtoAsync(cart);
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _cartRepository.FindOneAsync(c => c.UserId == userId);
            if (cart != null) return cart;

            var allCarts = await _cartRepository.GetAllAsync();
            var nextId = allCarts.Any() ? allCarts.Max(c => c.CartId) + 1 : 1;

            cart = new Cart
            {
                CartId = nextId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cartRepository.CreateAsync(cart);
            return cart;
        }

        private async Task TouchCartAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(c => c.CartId == cart.CartId, cart);
        }

        private async Task<CartDto> BuildCartDtoAsync(Cart cart)
        {
            var cartItemsEnum = await _cartItemRepository.FindManyAsync(ci => ci.CartId == cart.CartId);
            var cartItems = cartItemsEnum.ToList();

            var itemDtos = new List<CartItemDto>();
            var removedItemNotices = new List<string>();
            var adjustedItemNotices = new List<string>();

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product == null || product.Status == "sold_out")
                {
                    await _cartItemRepository.DeleteAsync(
                        ci => ci.CartId == cart.CartId
                           && ci.ProductId == item.ProductId
                           && ci.SizeName == item.SizeName);

                    removedItemNotices.Add(
                        product == null
                            ? $"Sản phẩm #{item.ProductId} ({item.SizeName}) không còn kinh doanh và đã được xóa khỏi giỏ hàng."
                            : $"\"{product.Name}\" ({item.SizeName}) đã hết hàng và được xóa khỏi giỏ hàng.");
                    continue;
                }

                var currentSize = product.Sizes.FirstOrDefault(
                    s => s.Name.Equals(item.SizeName, StringComparison.OrdinalIgnoreCase));

                if (currentSize == null)
                {
                    await _cartItemRepository.DeleteAsync(
                        ci => ci.CartId == cart.CartId
                           && ci.ProductId == item.ProductId
                           && ci.SizeName == item.SizeName);

                    removedItemNotices.Add(
                        $"\"{product.Name}\" size \"{item.SizeName}\" không còn được cung cấp và đã được xóa khỏi giỏ hàng.");
                    continue;
                }

                var quantity = item.Quantity;

                if (quantity > product.StockQuantity)
                {
                    quantity = product.StockQuantity;
                    item.Quantity = quantity;
                    await _cartItemRepository.UpdateAsync(
                        ci => ci.CartId == cart.CartId
                           && ci.ProductId == item.ProductId
                           && ci.SizeName == item.SizeName,
                        item);

                    adjustedItemNotices.Add(
                        $"\"{product.Name}\" ({item.SizeName}) đã được điều chỉnh xuống còn {quantity} do tồn kho không đủ.");
                }

                // Tính giá sale theo size cụ thể
                var (salePrice, hasPromotion) = await _promotionPriceHelper.GetSalePriceForSizeAsync(
                    product.ProductId, currentSize.Name, currentSize.Price);

                itemDtos.Add(new CartItemDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = currentSize.Price,
                    SalePrice = salePrice,
                    HasActivePromotion = hasPromotion,
                    SizeName = item.SizeName,
                    Quantity = quantity
                });
            }

            return new CartDto
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = itemDtos,
                RemovedItemNotices = removedItemNotices,
                AdjustedItemNotices = adjustedItemNotices,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }
    }
}