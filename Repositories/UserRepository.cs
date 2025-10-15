using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NoSQLproject.Repositories

{
    public class UserRepository : IUserRepository 
    {
        private static MongoClient? _client;
        private readonly IMongoCollection<User> _user;

        public UserRepository(IConfiguration config)
        {
            
            _client ??= new MongoClient(config["MongoDbSettings:ConnectionString"]);
            IMongoDatabase database = _client.GetDatabase(config["MongoDbSettings:DatabaseName"]);

            // access the "ticket" collection
            _user = database.GetCollection<User>("user");
        }

        public List<User> GetAllUsers()
        {
            return _user.Find(_ => true).ToList();
        }

        public User GetByEmployeeNumber(int employeeNumber)
        {
            return _user.Find(u => u.EmployeeNumber == employeeNumber).FirstOrDefault();
        }

        public void CreateUser(User user)
        {
             _user.InsertOne(user);
        }
        public void UpdateUser(User user) =>
            _user.ReplaceOne(u => u.Id == user.Id, user);

        public void DeleteUser(string id) =>
            _user.DeleteOne(u => u.Id == id);   // <-- N
        public User? GetByEmail(string email) =>
            _user.Find(u => u.Email == email).FirstOrDefault();
        public User? GetById(string id) => _user.Find(u => u.Id == id).FirstOrDefault();
    }
}
