
using NoSQLproject.Models;
using System.ComponentModel.DataAnnotations;

namespace NoSQLproject.Services.Interfaces
{
    public interface IUserService
    {
        // Queries
        List<User> GetAll();
        User GetByIdOrThrow(string id);

        // VM builder
        EditUserVm BuildEditVmOrThrow(string id);

        void Create(CreateUserVm vm);   
        void Update(EditUserVm vm);   
        void Delete(string id);
        List<User> Search(string? term);
    }
}
