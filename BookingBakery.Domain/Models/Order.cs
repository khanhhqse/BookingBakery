using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    public class OrderItem
    {
        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("unit_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal UnitPrice { get; set; }

        [BsonElement("total_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPrice { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("order_id")]
        public int OrderId { get; set; }

        [BsonElement("user_id")]
        public int UserId { get; set; }

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; } = new();

        [BsonElement("status")]
        public string Status { get; set; } = OrderStatus.ChoXacNhan;

        [BsonElement("total_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPrice { get; set; }

        [BsonElement("shipping_address")]
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>SĐT liên hệ khi giao hàng.</summary>
        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("note")]
        public string? Note { get; set; }

        [BsonElement("cancel_reason")]
        public string? CancelReason { get; set; }

        [BsonElement("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [BsonElement("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("status_history")]
        public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    }

    public class OrderStatusHistory
    {
        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("changed_by_user_id")]
        public int ChangedByUserId { get; set; }

        /// <summary>Lưu tên người thay đổi tại thời điểm ghi — tránh join sau này.</summary>
        [BsonElement("changed_by_user_name")]
        public string ChangedByUserName { get; set; } = string.Empty;

        [BsonElement("note")]
        public string? Note { get; set; }
    }

    public static class OrderStatus
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DangLam = "Đang làm";
        public const string DangGiao = "Đang giao";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";
    }

    public static class PaymentStatusConst
    {
        public const string ChoThanhToan = "Chờ thanh toán";
        public const string DaThanhToan = "Đã thanh toán";
        public const string ThanhToanThatBai = "Thanh toán thất bại";
    }
}