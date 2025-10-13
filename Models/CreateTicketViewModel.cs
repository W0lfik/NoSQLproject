using Microsoft.AspNetCore.Mvc.Rendering;

namespace NoSQLproject.Models;

public class CreateTicketViewModel
{
    public Ticket Ticket { get; set; } = new();

    // selections coming from the form
    public string? CreatedByUserId { get; set; }
    public List<string> HandledByUserIds { get; set; } = new();

    // data to render dropdowns
    public IEnumerable<SelectListItem> UsersSelect { get; set; } = new List<SelectListItem>();
}