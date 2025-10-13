using NoSQLproject.Models;

namespace NoSQLproject.Services.Interfaces
{
    public interface ITicketService
    {
        List<Ticket> GetFilteredAndSortedTickets(string searchEmail, string sortOrder);
    }
}