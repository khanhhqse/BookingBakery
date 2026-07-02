using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    [BsonIgnoreExtraElements]
    public class CartItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        // Composite key: (CartId, ProductId, SizeName) phải DUY NHẤT
        [BsonElement("cart_id")]
        public int CartId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        /// <summary>
        /// Size đã chọn — bắt buộc.
        /// Cùng 1 sản phẩm nhưng khác size = 2 CartItem riêng biệt.
        /// </summary>
        [BsonElement("size_name")]
        public string SizeName { get; set; } = string.Empty;

        /// <summary>Snapshot giá của size tại thời điểm thêm vào giỏ.</summary>
        [BsonElement("size_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SizePrice { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}