using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IOrderService
    {
        /// <summary>Customer đặt hàng từ Cart hiện tại (BR-O01).</summary>
        Task<(bool Success, string Message, OrderResponse? Order)> PlaceOrderAsync(
            int userId, PlaceOrderRequest request);

        /// <summary>Customer xem danh sách đơn hàng của mình.</summary>
        Task<(bool Success, string Message, List<OrderResponse>? Orders)> GetMyOrdersAsync(
            int userId);

        /// <summary>Xem chi tiết đơn hàng (Customer chỉ xem được đơn của mình).</summary>
        Task<(bool Success, string Message, OrderResponse? Order)> GetOrderDetailAsync(
            int orderId, int requestUserId, string userRole);

        /// <summary>Admin / Staff xem toàn bộ đơn hàng (FIFO — BR-O04).</summary>
        Task<(bool Success, string Message, List<OrderResponse>? Orders)> GetAllOrdersAsync(
            int page, int pageSize);

        /// <summary>Staff / Admin chuyển trạng thái đơn (BR-L01 — một chiều).</summary>
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(
            int orderId, UpdateOrderStatusRequest request, int actorUserId);

        /// <summary>
        /// Hủy đơn (BR-L04 / BR-L05 / BR-L06 / BR-L07).
        /// Customer: chỉ hủy được ở "Chờ xác nhận".
        /// Staff / Admin: hủy thêm được "Đang làm" (ghi nhận hao hụt).
        /// </summary>
        Task<(bool Success, string Message)> CancelOrderAsync(
            int orderId, CancelOrderRequest request, int actorUserId, string actorRole);

        /// <summary>
        /// Customer xác nhận đã nhận hàng (BR-L03).
        /// Chỉ được gọi khi đơn đang ở trạng thái "Đang giao".
        /// </summary>
        Task<(bool Success, string Message)> CustomerConfirmReceivedAsync(
            int orderId, int userId);
    }
}