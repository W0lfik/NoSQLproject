using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;


namespace NoSQLproject.Services
{
    public class LoginService : ILoginService
    {
        private ILoginRepository _loginRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginService(ILoginRepository loginRepository, IHttpContextAccessor httpContextAccessor)
        {
            _loginRepository = loginRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Register(User user)
        {
            _loginRepository.CreateUser(user);
        }

        public List<User> GetAllUsers()
        {
            return  _loginRepository.GetAllUsers();
        }

        public bool Login(int employeeNumber, string password)
        {
            var user = _loginRepository.GetByEmployeeNumber(employeeNumber);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public bool EmployeeNumberExists(int employeeNumber)
        {
            return _loginRepository.GetByEmployeeNumber(employeeNumber) != null;
        }
    }
}
