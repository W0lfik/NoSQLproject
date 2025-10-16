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

        public User GetByIdOrThrow(string id) =>
            _repo.GetById(id) ?? throw new KeyNotFoundException("User not found.");

        public EditUserVm BuildEditVmOrThrow(string id)
        {
            var u = GetByIdOrThrow(id);
            return new EditUserVm
            {
                Id = u.Id,
                FullName = u.FullName,
                EmployeeNumber = u.EmployeeNumber,
                Email = u.Email,
                Phone = u.Phone,
                Location = u.Location,
                TypeOfUser = u.TypeOfUser
            };
        }

        public void Create(CreateUserVm vm)
        {
            ValidateEmailUnique(vm.Email);

            var user = CreateUserFromVm(vm);
            AssignUniqueEmployeeNumber(user);
            _repo.CreateUser(user);
        }

        public void Update(EditUserVm vm)
        {
            var existing = GetByIdOrThrow(vm.Id);
            ValidateEmailUnique(vm.Email, vm.Id);

            UpdateUserFields(existing, vm);
            _repo.UpdateUser(existing);
        }

        public void Delete(string id)
        {
            var u = _repo.GetById(id);
            if (u == null) throw new KeyNotFoundException("User not found.");
            _repo.DeleteUser(id);
        }

        // ---------- Private helpers ----------

        private void ValidateEmailUnique(string email, string? excludeId = null)
        {
            var existing = _repo.GetByEmail(email);
            if (existing != null && existing.Id != excludeId)
                throw new ValidationException("Email already exists.");
        }

        private User CreateUserFromVm(CreateUserVm vm) => new()
        {
            FullName = vm.FullName?.Trim() ?? string.Empty,
            Email = vm.Email?.Trim() ?? string.Empty,
            Phone = vm.Phone?.Trim() ?? string.Empty,
            Location = vm.Location ?? string.Empty,
            TypeOfUser = vm.TypeOfUser,
            Password = BCrypt.Net.BCrypt.HashPassword(vm.Password ?? string.Empty)
        };

        private static void UpdateUserFields(User existing, EditUserVm vm)
        {
            existing.FullName = vm.FullName?.Trim() ?? string.Empty;
            existing.Email = vm.Email?.Trim() ?? string.Empty;
            existing.Phone = vm.Phone?.Trim() ?? string.Empty;
            existing.Location = vm.Location ?? string.Empty;
            existing.TypeOfUser = vm.TypeOfUser;

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
        }

        public List<User> Search(string? term)
        {
            var users = GetAll(); // already returns projected/safe fields

            if (string.IsNullOrWhiteSpace(term))
                return users;

            term = term.Trim();

            if (int.TryParse(term, out var empNo))
                return users.Where(u => u.EmployeeNumber == empNo).ToList();

            return users.Where(u =>
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }


        private void AssignUniqueEmployeeNumber(User user)
        {
            const int minInclusive = 100000;
            const int maxExclusive = 1_000_000;
            const int maxRetries = 10;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                user.EmployeeNumber = RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);
                try
                {
                    if (_repo.GetByEmployeeNumber(user.EmployeeNumber) == null) return;
                }
                catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                {
                    if (attempt == maxRetries)
                        throw new ValidationException("Could not allocate unique employee number.");
                }
            }
        }
    }
}
