
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using MongoDB.Driver;
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

        public User GetByIdOrThrow(string id)
        {
            var u = _repo.GetById(id);
            if (u == null) throw new KeyNotFoundException("User not found.");
            return u;
        }

        public EditUserVm BuildEditVmOrThrow(string id)
        {
            var u = GetByIdOrThrow(id);
            return new EditUserVm
            {
                Id = u.Id,
                FullName = u.FullName,
                EmployeeNumber = u.EmployeeNumber, // display-only in UI
                Email = u.Email,
                Phone = u.Phone,
                Location = u.Location,
                TypeOfUser = u.TypeOfUser
            };
        }

        public void Create(CreateUserVm vm)
        {
            if (_repo.GetByEmail(vm.Email) != null)
                throw new ValidationException("Email already exists.");

            var user = new User
            {
                FullName = (vm.FullName ?? string.Empty).Trim(),
                Email = (vm.Email ?? string.Empty).Trim(),
                Phone = vm.Phone?.Trim() ?? string.Empty,
                Location = vm.Location ?? string.Empty,
                TypeOfUser = vm.TypeOfUser,
                // Your model uses `Password` (not PasswordHash), so hash into that field:
                Password = BCrypt.Net.BCrypt.HashPassword(vm.Password ?? string.Empty)
            };

            // Generate a unique 6-digit EmployeeNumber with retry-on-duplicate
            const int minInclusive = 100000;
            const int maxExclusive = 1_000_000; // 6 digits
            const int maxRetries = 10;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                user.EmployeeNumber = RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);

                try
                {
                    _repo.CreateUser(user);
                    return; // success
                }
                catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                {
                    // Collision on unique index (EmployeeNumber). Try again.
                    if (attempt == maxRetries)
                        throw new ValidationException("Could not allocate a unique employee number. Please retry.");
                }
            }
        }

        public void Update(EditUserVm vm)
        {
            var existing = _repo.GetById(vm.Id);
            if (existing == null) throw new KeyNotFoundException("User not found.");

            // Enforce unique email (excluding this user)
            var byEmail = _repo.GetByEmail(vm.Email);
            if (byEmail != null && byEmail.Id != vm.Id)
                throw new ValidationException("Email already exists.");

            // EmployeeNumber is immutable: ignore any incoming value from the VM
            existing.FullName = (vm.FullName ?? string.Empty).Trim();
            existing.Email = (vm.Email ?? string.Empty).Trim();
            existing.Phone = vm.Phone?.Trim() ?? string.Empty;
            existing.Location = vm.Location ?? string.Empty;
            existing.TypeOfUser = vm.TypeOfUser;

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);

            _repo.UpdateUser(existing);
        }

        public void Delete(string id)
        {
            var u = _repo.GetById(id);
            if (u == null) throw new KeyNotFoundException("User not found.");
            _repo.DeleteUser(id);
        }
    }
}
