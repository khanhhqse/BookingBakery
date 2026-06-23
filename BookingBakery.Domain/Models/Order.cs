using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    /// <summary>
    /// Item nhúng vào Order (embedded document).
    /// Lưu snapshot giá tại thời điểm đặt hàng — không bị ảnh hưởng khi Product thay đổi sau.
    /// </summary>
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

        /// <summary>
        /// Vòng đời: "Chờ xác nhận" → "Đang làm" → "Đang giao" → "Hoàn thành"
        /// Hoặc "Đã hủy" tại các bước cho phép.
        /// </summary>
        [BsonElement("status")]
        public string Status { get; set; } = OrderStatus.ChoXacNhan;

        [BsonElement("total_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPrice { get; set; }

        [BsonElement("shipping_address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [BsonElement("note")]
        public string? Note { get; set; }

        /// <summary>Lý do hủy — bắt buộc khi hủy đơn (BR-L06).</summary>
        [BsonElement("cancel_reason")]
        public string? CancelReason { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Lịch sử thay đổi trạng thái (BR-L06).</summary>
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

        [BsonElement("note")]
        public string? Note { get; set; }
    }

    /// <summary>Hằng số trạng thái đơn hàng — dùng chung toàn project.</summary>
    public static class OrderStatus
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DangLam = "Đang làm";
        public const string DangGiao = "Đang giao";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";
    }
}