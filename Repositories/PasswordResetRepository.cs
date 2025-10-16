using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;

namespace NoSQLproject.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly IMongoCollection<PasswordResetToken> _tokens;

        public PasswordResetRepository(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _tokens = db.GetCollection<PasswordResetToken>("password_reset_tokens");
        }

        public void Create(PasswordResetToken token) => _tokens.InsertOne(token);

        public PasswordResetToken? GetByToken(string token) =>
            _tokens.Find(Builders<PasswordResetToken>.Filter.Eq("token", token)).FirstOrDefault();

        public void MarkUsed(string id) =>
            _tokens.UpdateOne(Builders<PasswordResetToken>.Filter.Eq("_id", id),
                              Builders<PasswordResetToken>.Update.Set(t => t.Used, true));

        public void InvalidateAllForUser(string userId) =>
            _tokens.UpdateMany(Builders<PasswordResetToken>.Filter.Eq("userId", userId),
                               Builders<PasswordResetToken>.Update.Set(t => t.Used, true));
    }
}

