using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, OrderResponse? Order)> PlaceOrderAsync(
            int userId, PlaceOrderRequest request);

        Task<(bool Success, string Message, List<OrderSummaryResponse>? Orders)> GetMyOrdersAsync(
            int userId);

        Task<(bool Success, string Message, OrderResponse? Order)> GetOrderDetailAsync(
            int orderId, int requestUserId, string userRole);

        Task<(bool Success, string Message, List<OrderSummaryResponse>? Orders)> GetAllOrdersAsync(
            GetAllOrdersRequest request);

        Task<(bool Success, string Message)> UpdateOrderStatusAsync(
            int orderId, UpdateOrderStatusRequest request, int actorUserId, string actorUserName);

        Task<(bool Success, string Message)> CancelOrderAsync(
            int orderId, CancelOrderRequest request, int actorUserId, string actorUserName, string actorRole);

        Task<(bool Success, string Message)> CustomerConfirmReceivedAsync(
            int orderId, int userId, string userName);

        /// <summary>
        /// Customer cập nhật SĐT và địa chỉ giao hàng khi đơn đang ở "Chờ xác nhận".
        /// Phải cung cấp ít nhất một trong hai field.
        /// </summary>
        Task<(bool Success, string Message, OrderResponse? Order)> UpdateOrderContactAsync(
            int orderId, UpdateOrderContactRequest request, int userId);
    }
}