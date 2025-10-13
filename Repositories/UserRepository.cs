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

    }
}
