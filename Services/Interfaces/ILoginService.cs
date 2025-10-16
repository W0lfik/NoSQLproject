using System.Security.Claims;
using NoSQLproject.Models;

namespace NoSQLproject.Services.Interfaces
{
    public interface ILoginService
    {
        // existing
        List<User> GetAllUsers();
        void Register(User user);
        bool EmployeeNumberExists(int employeeNumber);

        // new (for thin controller)
        User? ValidateCredentials(int employeeNumber, string password);
        ClaimsPrincipal CreatePrincipal(User user);
    }
}
