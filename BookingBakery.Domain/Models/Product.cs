using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
    /// <summary>Size embedded vào Product — mỗi size có giá riêng.</summary>
    public class ProductSize
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectId { get; set; }

        [BsonElement("product_id")]
        public int ProductId { get; set; }

        [BsonElement("category_id")]
        public int CategoryId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        /// <summary>Hướng dẫn bảo quản sản phẩm.</summary>
        [BsonElement("storage_instructions")]
        public string? StorageInstructions { get; set; }

        /// <summary>
        /// Giá gốc của product — dùng làm fallback nếu cần.
        /// Giá thực tế lấy từ Size được chọn.
        /// </summary>
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("cost_price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CostPrice { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("image_url")]
        public string? ImageUrl { get; set; }

        /// <summary>"stock" | "sold_out".</summary>
        [BsonElement("status")]
        public string Status { get; set; } = "stock";

        /// <summary>
        /// Danh sách size với giá riêng. Bắt buộc ít nhất 1 size.
        /// VD: [{ name: "S", price: 20000 }, { name: "M", price: 25000 }]
        /// </summary>
        [BsonElement("sizes")]
        public List<ProductSize> Sizes { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}