using NoSQLproject.Models;

namespace NoSQLproject.Services.Interfaces
{
    public interface IUserService
    {
        List<User> GetAll();
        User? GetById(string id);
        User? GetByEmail(string email);
        User? GetByEmployeeNumber(int employeeNumber);

        bool Create(User userPlain);
        bool Update(User userPlain);
        bool Delete(string id);
    }
}
