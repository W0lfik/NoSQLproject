using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;

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
            _user = database.GetCollection<User>("user");
        }

        //This is safe and efficient (Guys! follow this way to get data from the database 'Qasem')
        public List<User> GetAllUsers()
        {
            var projection = GetBasicUserProjection();
            return _user.Find(_ => true)
                        .Project<User>(projection)
                        .ToList();
        }

        public List<User> GetUsersByRole(TypeOfUser role)
        {
            var projection = GetBasicUserProjection();
            return _user.Find(u => u.TypeOfUser == role)
                        .Project<User>(projection)
                        .ToList();
        }

        // Private helper for reusable projection logic
        private static ProjectionDefinition<User> GetBasicUserProjection()
        {
            return Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.FullName)
                .Include(u => u.Email)
                .Include(u => u.EmployeeNumber)
                .Include(u => u.TypeOfUser)
                .Include(u => u.Location);
               
        }



        public User? GetByEmployeeNumber(int employeeNumber)
        {
            return _user.Find(u => u.EmployeeNumber == employeeNumber).FirstOrDefault();
        }

        public void CreateUser(User user)
        {
            _user.InsertOne(user);
        }

        public void UpdateUser(User user)
        {
            _user.ReplaceOne(u => u.Id == user.Id, user);
        }

        public void DeleteUser(string id)
        {
            _user.DeleteOne(u => u.Id == id);
        }

        public User? GetByEmail(string email)
        {
            return _user.Find(u => u.Email == email).FirstOrDefault();
        }

        public User? GetById(string id)
        {
            return _user.Find(u => u.Id == id).FirstOrDefault();
        }
    }
}
