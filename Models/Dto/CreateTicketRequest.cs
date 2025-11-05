using System.ComponentModel.DataAnnotations;
using NoSQLproject.Models;

namespace NoSQLproject.Models.Dto;

public class CreateTicketRequest
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public IncidentType IncidentType { get; set; }

    public Priority? Priority { get; set; }

    public UserInTicketDto? CreatedBy { get; set; }

    public List<UserInTicketDto> HandledBy { get; set; } = new();
}
