using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingBakery.Domain.Models
{
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

        /// <summary>Size của sản phẩm này. Mỗi ProductId = 1 size riêng biệt.</summary>
        [BsonElement("size_name")]
        public string SizeName { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        /// <summary>Hướng dẫn bảo quản.</summary>
        [BsonElement("storage_instructions")]
        public string? StorageInstructions { get; set; }

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

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}