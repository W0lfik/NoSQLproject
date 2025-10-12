using NoSQLproject.Models;

namespace NoSQLproject.Services.Interfaces
{
    public interface ILoginService
    {
        public List<User> GetAllUsers();
        public void Register(User user);
        public bool Login(int employeeNumber, string password);
        bool EmployeeNumberExists(int employeeNumber);
    }
}
