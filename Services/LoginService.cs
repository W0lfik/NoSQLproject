using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;


namespace NoSQLproject.Services
{
    public class LoginService : ILoginService
    {
        private IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Register(User user)
        {
            _userRepository.CreateUser(user);
        }

        public List<User> GetAllUsers()
        {
            return  _userRepository.GetAllUsers();
        }

        public bool Login(int employeeNumber, string password)
        {
            var user = _userRepository.GetByEmployeeNumber(employeeNumber);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public bool EmployeeNumberExists(int employeeNumber)
        {
            return _userRepository.GetByEmployeeNumber(employeeNumber) != null;
        }
    }
}
