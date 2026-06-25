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
            var cart = await _cartRepo.FindOneAsync(c => c.UserId == userId);
            if (cart == null)
                return (false,
                    "Bạn chưa có giỏ hàng. Vui lòng thêm sản phẩm vào giỏ trước khi đặt hàng.", null);

            var cartItemsEnum = await _cartItemRepo.FindManyAsync(ci => ci.CartId == cart.CartId);
            var cartItems = cartItemsEnum.ToList();

            if (cartItems.Count == 0)
                return (false,
                    "Giỏ hàng của bạn đang trống. Vui lòng thêm ít nhất một sản phẩm trước khi đặt hàng.", null);

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

            var paymentMethod = request.PaymentMethod switch
            {
                PaymentMethodOption.COD => "COD",
                PaymentMethodOption.BankTransfer => "Chuyển khoản",
                _ => "COD"
            };

            var order = new Order
            {
                OrderId = orderId,
                UserId = userId,
                Items = orderItems,
                Status = OrderStatus.ChoXacNhan,
                TotalPrice = totalPrice,
                ShippingAddress = request.ShippingAddress,
                Note = request.Note,
                PaymentMethod = paymentMethod,
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

            // Xóa cart items sau khi đặt hàng thành công
            await _cartItemRepo.DeleteManyAsync(ci => ci.CartId == cart.CartId);

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

            if (userRole == "3" && order.UserId != requestUserId)
                return (false, "Bạn không có quyền xem đơn hàng này.", null);

            return (true, "Lấy chi tiết đơn hàng thành công.", MapToResponse(order));
        }

        // ──────────────────────────────────────────────────────────────
        // 4. XEM TẤT CẢ ĐƠN (Admin / Staff — FIFO + filter)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, List<OrderResponse>? Orders)> GetAllOrdersAsync(
            GetAllOrdersRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 20;

            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (request.Today == true)
            {
                var todayVn = DateTime.UtcNow.AddHours(7).Date;
                fromDate = todayVn.AddHours(-7);
                toDate = todayVn.AddHours(17).AddSeconds(-1);
            }
            else
            {
                if (request.FromDate.HasValue)
                    fromDate = DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc)
                                       .AddHours(-7);

                if (request.ToDate.HasValue)
                    toDate = DateTime.SpecifyKind(request.ToDate.Value.Date, DateTimeKind.Utc)
                                     .AddHours(17).AddSeconds(-1);
            }

            var orders = await _orderRepo.GetAllAsync(
                request.Page, request.PageSize, request.Status, fromDate, toDate);
            var responses = orders.Select(MapToResponse).ToList();

            return (true, $"Lấy danh sách đơn hàng thành công (trang {request.Page}).", responses);
        }

        // ──────────────────────────────────────────────────────────────
        // 5. CẬP NHẬT TRẠNG THÁI — Staff / Admin (BR-L01, cho bỏ qua bước)
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

            var requestedStatus = request.NewStatus switch
            {
                OrderStatusOption.DangLam => OrderStatus.DangLam,
                OrderStatusOption.DangGiao => OrderStatus.DangGiao,
                OrderStatusOption.HoanThanh => OrderStatus.HoanThanh,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(requestedStatus))
                return (false, "Trạng thái không hợp lệ.");

            var statusOrder = new Dictionary<string, int>
            {
                [OrderStatus.ChoXacNhan] = 1,
                [OrderStatus.DangLam] = 2,
                [OrderStatus.DangGiao] = 3,
                [OrderStatus.HoanThanh] = 4,
                [OrderStatus.DaHuy] = 99
            };

            if (!statusOrder.TryGetValue(order.Status, out var currentLevel) ||
                !statusOrder.TryGetValue(requestedStatus, out var requestedLevel))
                return (false, "Trạng thái không hợp lệ.");

            if (requestedLevel <= currentLevel)
                return (false,
                    $"Không thể chuyển đơn hàng từ \"{order.Status}\" về \"{requestedStatus}\". " +
                    "Trạng thái chỉ được chuyển theo chiều thuận.");

            // BR-L02: Trừ stock khi đi qua bước "Đang làm" dù bỏ qua hay không
            bool skipDangLam = currentLevel < statusOrder[OrderStatus.DangLam]
                            && requestedLevel > statusOrder[OrderStatus.DangLam];
            bool goToDangLam = requestedStatus == OrderStatus.DangLam;

            if (goToDangLam || skipDangLam)
            {
                var deductResult = await DeductStockAsync(order);
                if (!deductResult.Success)
                    return (false, deductResult.Message);
            }

            // Ghi nhận thời điểm giao hàng để tính auto-complete 48h (BR-L03)
            if (requestedStatus == OrderStatus.DangGiao)
                order.DeliveredAt = DateTime.UtcNow;

            AppendStatusHistory(order, requestedStatus, actorUserId, request.Note);
            order.Status = requestedStatus;
            await _orderRepo.UpdateAsync(order);

            return (true,
                $"Đơn hàng #{orderId} đã được chuyển sang trạng thái \"{requestedStatus}\" thành công.");
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

            if (isStaffOrAdmin)
            {
                var cancellableStatuses = new[] { OrderStatus.ChoXacNhan, OrderStatus.DangLam };
                if (!cancellableStatuses.Contains(order.Status))
                    return (false,
                        $"Đơn hàng #{orderId} đang ở trạng thái \"{order.Status}\", " +
                        "không thể thực hiện hủy ở giai đoạn này.");
            }

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
        // 7. CUSTOMER XÁC NHẬN ĐÃ NHẬN HÀNG (BR-L03)
        // ──────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> CustomerConfirmReceivedAsync(
            int orderId, int userId)
        {
            var order = await _orderRepo.GetByOrderIdAsync(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.UserId != userId)
                return (false, "Bạn không có quyền xác nhận đơn hàng này.");

            if (order.Status != OrderStatus.DangGiao)
                return (false,
                    $"Đơn hàng #{orderId} đang ở trạng thái \"{order.Status}\". " +
                    "Bạn chỉ có thể xác nhận đã nhận hàng khi đơn đang được giao thôi nhé.");

            AppendStatusHistory(order, OrderStatus.HoanThanh, userId,
                "Khách hàng xác nhận đã nhận hàng");
            order.Status = OrderStatus.HoanThanh;
            await _orderRepo.UpdateAsync(order);

            return (true,
                "Cảm ơn bạn đã xác nhận! Đơn hàng đã hoàn thành. " +
                "Chúc bạn thưởng thức ngon miệng nhé!");
        }

        // ──────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ──────────────────────────────────────────────────────────────

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
                    $"Không thể xử lý đơn #{order.OrderId} vì nguyên liệu không đủ: " +
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
            PaymentMethod = o.PaymentMethod,
            DeliveredAt = o.DeliveredAt,
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