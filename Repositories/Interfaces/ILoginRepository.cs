using NoSQLproject.Models;

namespace NoSQLproject.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        List<User> GetAllUsers();
        User GetByEmployeeNumber(int employeeNumber);
        void CreateUser(User user);
        
    }
}
