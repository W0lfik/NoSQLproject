using NoSQLproject.Models;

namespace NoSQLproject.Models.Dto;

public class UpdateTicketRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IncidentType? IncidentType { get; set; }

    public Priority? Priority { get; set; }

    public State? State { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public UserInTicketDto? CreatedBy { get; set; }

    public List<UserInTicketDto>? HandledBy { get; set; }
}
