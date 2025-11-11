using NoSQLproject.Models;

namespace NoSQLproject.Repositories.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        List<User> GetUsersByRole(TypeOfUser role);
        User? GetById(string id);
        User? GetByEmployeeNumber(int employeeNumber);
        User? GetByEmail(string email);
        void CreateUser(User user);
        void UpdateUser(User user);
        void DeleteUser(string id);   // <-- NEW
    }
}
