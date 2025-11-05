using System.ComponentModel.DataAnnotations;
using NoSQLproject.Models;

namespace NoSQLproject.Models.Dto;

public class UserInTicketDto
{
    [Required]
    public required string Id { get; set; }

    [Required]
    public required string FullName { get; set; }

    [Required]
    public required int EmployeeNumber { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required TypeOfUser TypeOfUser { get; set; }

    public UserInTicket ToModel()
    {
        return new UserInTicket
        {
            Id = Id,
            FullName = FullName,
            EmployeeNumber = EmployeeNumber,
            Email = Email,
            TypeOfUser = TypeOfUser
        };
    }
}
