using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models
{
    public class PasswordResetToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("token")]
        public string Token { get; set; } = string.Empty; // url-safe random string

        [BsonElement("expiresAtUtc")]
        public DateTime ExpiresAtUtc { get; set; }        // e.g. now + 30 minutes

        [BsonElement("used")]
        public bool Used { get; set; } = false;
    }
}

