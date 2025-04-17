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

        [BsonElement("isFreeAvailable")]
        public bool IsFreeAvailable { get; set; } = true;

        [BsonElement("purchasesHistory")]
        public List<Purchase>? PurchasesHistory { get; set; }

        [BsonElement("connectionHistory")]
        public List<Connection>? ConnectionHistory { get; set; }

        [BsonElement("subscriptionEndDateUtc")]
        public DateTime? SubscriptionEndDateUtc { get; set; }

        [BsonElement("isAdmin")] 
        public bool IsAdmin { get; set; } = false;

        [BsonElement("createdAtUtc")]
        public required DateTime CreatedAtUtc { get; set; }
    }
}