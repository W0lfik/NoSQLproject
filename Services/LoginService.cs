using System.Security.Claims;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _users;

        public LoginService(IUserRepository users)
        {
            _users = users;
        }

        // Create a user (expects password already hashed by caller;
        // move the hash here if you prefer centralizing it)
        public void Register(User user) => _users.CreateUser(user);

        // Simple queries
        public List<User> GetAllUsers() => _users.GetAllUsers();

        public bool EmployeeNumberExists(int employeeNumber) =>
            _users.GetByEmployeeNumber(employeeNumber) != null;

        // ---- Auth helpers for a thin controller ----

        // Returns the user if credentials are valid; otherwise null
        public User? ValidateCredentials(int employeeNumber, string password)
        {
            var user = _users.GetByEmployeeNumber(employeeNumber);
            if (user == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, user.Password) ? user : null;
        }

        // Build the ClaimsPrincipal for cookie auth
        public ClaimsPrincipal CreatePrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("EmployeeNumber", user.EmployeeNumber.ToString()),
                new Claim(ClaimTypes.Role, user.TypeOfUser.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
