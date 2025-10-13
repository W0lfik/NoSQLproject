using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        public UserService(IUserRepository repo) => _repo = repo;

        public List<User> GetAll() => _repo.GetAllUsers();
        public User? GetById(string id) => _repo.GetById(id);
        public User? GetByEmail(string email) => _repo.GetByEmail(email);
        public User? GetByEmployeeNumber(int emp) => _repo.GetByEmployeeNumber(emp);

        public bool Create(User userPlain)
        {
            if (_repo.GetByEmail(userPlain.Email) != null) return false;
            if (_repo.GetByEmployeeNumber(userPlain.EmployeeNumber) != null) return false;
            userPlain.Password = BCrypt.Net.BCrypt.HashPassword(userPlain.Password);
            _repo.CreateUser(userPlain);
            return true;
        }

        public bool Update(User userPlain)
        {
            var existing = _repo.GetById(userPlain.Id);
            if (existing == null) return false;

            var byEmail = _repo.GetByEmail(userPlain.Email);
            if (byEmail != null && byEmail.Id != userPlain.Id) return false;

            var byEmp = _repo.GetByEmployeeNumber(userPlain.EmployeeNumber);
            if (byEmp != null && byEmp.Id != userPlain.Id) return false;

            if (!string.IsNullOrWhiteSpace(userPlain.Password) && !userPlain.Password.StartsWith("$2"))
                userPlain.Password = BCrypt.Net.BCrypt.HashPassword(userPlain.Password);
            else
                userPlain.Password = existing.Password;

            _repo.UpdateUser(userPlain);
            return true;
        }

        public bool Delete(string id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return false;
            _repo.DeleteUser(id);
            return true;
        }
    }
}
