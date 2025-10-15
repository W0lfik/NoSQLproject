// File: Models/UserVms.cs
using System.ComponentModel.DataAnnotations;

namespace NoSQLproject.Models
{
    public class CreateUserVm
    {
        [Required] public string FullName { get; set; } = string.Empty;
        // EmployeeNumber removed (auto-generated in service)
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Phone] public string Phone { get; set; } = string.Empty;
        public string? Location { get; set; }
        [Required] public TypeOfUser TypeOfUser { get; set; } = TypeOfUser.employee;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    }

    public class EditUserVm
    {
        [Required] public string Id { get; set; } = string.Empty;
        [Required] public string FullName { get; set; } = string.Empty;

        // Display-only, not editable
        public int EmployeeNumber { get; set; }

        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Phone] public string Phone { get; set; } = string.Empty;
        public string? Location { get; set; }
        [Required] public TypeOfUser TypeOfUser { get; set; } = TypeOfUser.employee;

        [DataType(DataType.Password)] public string? NewPassword { get; set; }
    }
}
