using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;

namespace BookingBakery.Application.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly ICartItemRepository _cartItemRepo;
        private readonly IProductRepository _productRepo;

        public OrderService(
            IOrderRepository orderRepo,
            ICartRepository cartRepo,
            ICartItemRepository cartItemRepo,
            IProductRepository productRepo)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
        }

        // ──────────────────────────────────────────────────────────────
        // 1. ĐẶT HÀNG (BR-O01)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, OrderResponse? Order)> PlaceOrderAsync(
            int userId, PlaceOrderRequest request)
        {
            // Lấy cart của user
            var cart = await _cartRepo.FindOneAsync(c => c.UserId == userId);
            if (cart == null)
                return (false,
                    "Bạn chưa có giỏ hàng. Vui lòng thêm sản phẩm vào giỏ trước khi đặt hàng.", null);

            // Lấy items trong cart
            var cartItemsEnum = await _cartItemRepo.FindManyAsync(ci => ci.CartId == cart.CartId);
            var cartItems = cartItemsEnum.ToList();

            if (cartItems.Count == 0)
                return (false,
                    "Giỏ hàng của bạn đang trống. Vui lòng thêm ít nhất một sản phẩm trước khi đặt hàng.", null);

            // Validate từng sản phẩm
            var orderItems = new List<OrderItem>();
            var unavailableProducts = new List<string>();
            var insufficientStockProducts = new List<string>();

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepo.GetByIdAsync(cartItem.ProductId);

                if (product == null || product.Status == "sold_out")
                {
                    unavailableProducts.Add($"sản phẩm #{cartItem.ProductId}");
                    continue;
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    insufficientStockProducts.Add($"\"{product.Name}\"");
                    continue;
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * cartItem.Quantity
                });
            }

            if (unavailableProducts.Count > 0)
                return (false,
                    $"Rất tiếc, một số sản phẩm trong giỏ hàng đã hết hoặc không còn kinh doanh: " +
                    $"{string.Join(", ", unavailableProducts)}. " +
                    "Vui lòng xóa các sản phẩm này khỏi giỏ hàng rồi thử lại nhé.", null);

            if (insufficientStockProducts.Count > 0)
                return (false,
                    $"Rất tiếc, số lượng trong kho không đủ cho: {string.Join(", ", insufficientStockProducts)}. " +
                    "Vui lòng điều chỉnh số lượng trong giỏ hàng rồi thử lại nhé.", null);

            if (orderItems.Count == 0)
                return (false,
                    "Không có sản phẩm hợp lệ nào để đặt hàng. Vui lòng kiểm tra lại giỏ hàng.", null);

            var totalPrice = orderItems.Sum(i => i.TotalPrice);
            var orderId = await _orderRepo.GetNextOrderIdAsync();
            var now = DateTime.UtcNow;

            var order = new Order
            {
                OrderId = orderId,
                UserId = userId,
                Items = orderItems,
                Status = OrderStatus.ChoXacNhan,
                TotalPrice = totalPrice,
                ShippingAddress = request.ShippingAddress,
                Note = request.Note,
                CreatedAt = now,
                UpdatedAt = now,
                StatusHistory = new List<OrderStatusHistory>
                {
                    new()
                    {
                        Status          = OrderStatus.ChoXacNhan,
                        ChangedAt       = now,
                        ChangedByUserId = userId,
                        Note            = "Khách hàng tạo đơn hàng"
                    }
                }
            };

            await _orderRepo.CreateAsync(order);

            return (true,
                "Đặt hàng thành công! Đơn hàng của bạn đang chờ nhân viên xác nhận.",
                MapToResponse(order));
        }

        // ──────────────────────────────────────────────────────────────
        // 2. XEM ĐƠN HÀNG CỦA MÌNH (Customer)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<OrderResponse>? Orders)> GetMyOrdersAsync(
            int userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);
            var responses = orders.Select(MapToResponse).ToList();
            return (true, "Lấy danh sách đơn hàng thành công.", responses);
        }

        // ──────────────────────────────────────────────────────────────
        // 3. XEM CHI TIẾT ĐƠN HÀNG
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, OrderResponse? Order)> GetOrderDetailAsync(
            int orderId, int requestUserId, string userRole)
        {
            var order = await _orderRepo.GetByOrderIdAsync(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.", null);

            // Customer (role "3") chỉ xem được đơn của mình
            if (userRole == "3" && order.UserId != requestUserId)
                return (false, "Bạn không có quyền xem đơn hàng này.", null);

            return (true, "Lấy chi tiết đơn hàng thành công.", MapToResponse(order));
        }

        // ──────────────────────────────────────────────────────────────
        // 4. XEM TẤT CẢ ĐƠN (Admin / Staff — FIFO)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<OrderResponse>? Orders)> GetAllOrdersAsync(
            int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var orders = await _orderRepo.GetAllAsync(page, pageSize);
            var responses = orders.Select(MapToResponse).ToList();
            return (true, $"Lấy danh sách đơn hàng thành công (trang {page}).", responses);
        }

        // ──────────────────────────────────────────────────────────────
        // 5. CẬP NHẬT TRẠNG THÁI — Staff / Admin (BR-L01 — một chiều)
        //    Chờ xác nhận → Đang làm → Đang giao → Hoàn thành
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(
            int orderId, UpdateOrderStatusRequest request, int actorUserId)
        {
            var order = await _orderRepo.GetByOrderIdAsync(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.Status == OrderStatus.DaHuy)
                return (false, $"Đơn hàng #{orderId} đã bị hủy, không thể cập nhật trạng thái.");

            if (order.Status == OrderStatus.HoanThanh)
                return (false, $"Đơn hàng #{orderId} đã hoàn thành, không thể cập nhật trạng thái.");

            var allowedTransitions = GetAllowedTransitions();

            if (!allowedTransitions.TryGetValue(order.Status, out var nextAllowedStatus))
                return (false,
                    $"Đơn hàng #{orderId} đang ở trạng thái \"{order.Status}\" và không thể chuyển thêm.");

            // Map enum -> string constant để so sánh
            var requestedStatus = request.NewStatus switch
            {
                OrderStatusOption.DangLam => OrderStatus.DangLam,
                OrderStatusOption.DangGiao => OrderStatus.DangGiao,
                OrderStatusOption.HoanThanh => OrderStatus.HoanThanh,
                _ => string.Empty
            };

            if (requestedStatus != nextAllowedStatus)
                return (false,
                    $"Trạng thái không hợp lệ. Từ \"{order.Status}\", " +
                    $"đơn hàng chỉ có thể chuyển sang \"{nextAllowedStatus}\".");

            // BR-L02: Khi bắt đầu làm → trừ stock
            if (requestedStatus == OrderStatus.DangLam)
            {
                var deductResult = await DeductStockAsync(order);
                if (!deductResult.Success)
                    return (false, deductResult.Message);
            }

            AppendStatusHistory(order, nextAllowedStatus, actorUserId, request.Note);
            order.Status = nextAllowedStatus;
            await _orderRepo.UpdateAsync(order);

            return (true,
                $"Đơn hàng #{orderId} đã được chuyển sang trạng thái \"{nextAllowedStatus}\" thành công.");
        }

        // ──────────────────────────────────────────────────────────────
        // 6. HỦY ĐƠN (BR-L04 / BR-L05 / BR-L06 / BR-L07)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> CancelOrderAsync(
            int orderId, CancelOrderRequest request, int actorUserId, string actorRole)
        {
            var order = await _orderRepo.GetByOrderIdAsync(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.Status == OrderStatus.DaHuy)
                return (false, $"Đơn hàng #{orderId} đã được hủy trước đó rồi.");

            if (order.Status == OrderStatus.HoanThanh)
                return (false, $"Đơn hàng #{orderId} đã hoàn thành, không thể hủy.");

            bool isCustomer = actorRole == "3";
            bool isStaffOrAdmin = actorRole == "1" || actorRole == "2";

            // BR-L07: Customer chỉ hủy được ở "Chờ xác nhận"
            if (isCustomer)
            {
                if (order.UserId != actorUserId)
                    return (false, "Bạn không có quyền hủy đơn hàng này.");

                if (order.Status != OrderStatus.ChoXacNhan)
                    return (false,
                        $"Rất tiếc, đơn hàng #{orderId} đang ở trạng thái \"{order.Status}\". " +
                        "Bạn chỉ có thể hủy đơn khi đơn đang ở trạng thái \"Chờ xác nhận\" thôi nhé. " +
                        "Nếu cần hỗ trợ thêm, vui lòng liên hệ nhân viên của chúng tôi.");
            }

            // Staff / Admin: hủy được ở "Chờ xác nhận" và "Đang làm"
            if (isStaffOrAdmin)
            {
                var cancellableStatuses = new[]
                {
                    OrderStatus.ChoXacNhan,
                    OrderStatus.DangLam    // BR-L05: ghi nhận hao hụt
                };

                if (!cancellableStatuses.Contains(order.Status))
                    return (false,
                        $"Đơn hàng #{orderId} đang ở trạng thái \"{order.Status}\", " +
                        "không thể thực hiện hủy ở giai đoạn này.");
            }

            // BR-L05: Hủy sau khi đang làm → ghi nhận hao hụt, KHÔNG hoàn kho
            var historyNote = order.Status == OrderStatus.DangLam
                ? $"[Hao hụt ghi nhận] Hủy sau khi đang làm. Lý do: {request.CancelReason}"
                : $"Lý do hủy: {request.CancelReason}";

            AppendStatusHistory(order, OrderStatus.DaHuy, actorUserId, historyNote);
            order.Status = OrderStatus.DaHuy;
            order.CancelReason = request.CancelReason;
            await _orderRepo.UpdateAsync(order);

            return (true,
                isCustomer
                    ? $"Đơn hàng #{orderId} đã được hủy thành công. " +
                      "Nếu bạn có thắc mắc gì, đừng ngần ngại liên hệ với chúng tôi nhé!"
                    : $"Đã hủy đơn hàng #{orderId}. Lý do đã được ghi nhận vào lịch sử đơn hàng.");
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

        private static Dictionary<string, string> GetAllowedTransitions() => new()
        {
            [OrderStatus.ChoXacNhan] = OrderStatus.DangLam,
            [OrderStatus.DangLam] = OrderStatus.DangGiao,
            [OrderStatus.DangGiao] = OrderStatus.HoanThanh,
        };

        /// <summary>
        /// BR-L02: Trừ stock khi chuyển sang "Đang làm".
        /// Pass 1: kiểm tra đủ stock toàn bộ. Pass 2: mới trừ.
        /// </summary>
        private async Task<(bool Success, string Message)> DeductStockAsync(Order order)
        {
            var insufficientItems = new List<string>();

            foreach (var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                    insufficientItems.Add($"\"{item.ProductName}\"");
            }

            if (insufficientItems.Count > 0)
                return (false,
                    $"Không thể bắt đầu làm đơn #{order.OrderId} vì nguyên liệu không đủ: " +
                    $"{string.Join(", ", insufficientItems)}. Vui lòng kiểm tra lại tồn kho.");

            foreach (var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null) continue;

                product.StockQuantity -= item.Quantity;
                if (product.StockQuantity <= 0)
                {
                    product.StockQuantity = 0;
                    product.Status = "sold_out";
                }
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepo.UpdateAsync(p => p.ProductId == item.ProductId, product);
            }

            return (true, string.Empty);
        }

        private static void AppendStatusHistory(
            Order order, string newStatus, int changedByUserId, string? note)
        {
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Note = note
            });
        }

        private static OrderResponse MapToResponse(Order o) => new()
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            Items = o.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            Status = o.Status,
            TotalPrice = o.TotalPrice,
            ShippingAddress = o.ShippingAddress,
            Note = o.Note,
            CancelReason = o.CancelReason,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            StatusHistory = o.StatusHistory.Select(h => new OrderStatusHistoryResponse
            {
                Status = h.Status,
                ChangedAt = h.ChangedAt,
                ChangedByUserId = h.ChangedByUserId,
                Note = h.Note
            }).ToList()
        };
    }
}