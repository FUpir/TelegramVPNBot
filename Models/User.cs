using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelegramVPNBot.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("telegramId")]
        public required long TelegramId { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("fullName")]
        public required string FullName { get; set; }

        [BsonElement("settings")]
        public required Settings Settings { get; set; }

        [BsonElement("outLineKey")]
        public string? OutlineKey { get; set; }

        [BsonElement("purchasesHistory")]
        public List<Purchase>? PurchasesHistory { get; set; }

        [BsonElement("subscriptionEndDateUtc")]
        public DateTime? SubscriptionEndDateUtc { get; set; }

        [BsonElement("createdAtUtc")]
        public required DateTime CreatedAtUtc { get; set; }
    }
}