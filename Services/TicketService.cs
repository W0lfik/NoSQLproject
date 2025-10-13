using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public List<Ticket> GetFilteredAndSortedTickets(string searchEmail, string sortOrder)
        {
            var tickets = _ticketRepository.GetAllTickets();

            // search by email
            if (!string.IsNullOrEmpty(searchEmail))
            {
                tickets = tickets
                    .Where(t =>
                        t.CreatedBy != null &&
                        t.CreatedBy.Email != null &&
                        t.CreatedBy.Email.Contains(searchEmail, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // asc-desc
            tickets = sortOrder switch
            {
                "id_desc" => tickets.OrderByDescending(t => t.TicketNumber).ToList(),
                "id_asc" => tickets.OrderBy(t => t.TicketNumber).ToList(),

                "user_desc" => tickets.OrderByDescending(t => t.CreatedBy?.Email).ToList(),
                "user_asc" => tickets.OrderBy(t => t.CreatedBy?.Email).ToList(),

                "date_desc" => tickets.OrderByDescending(t => t.CreatedAt).ToList(),
                "date_asc" => tickets.OrderBy(t => t.CreatedAt).ToList(),

                _ => tickets.OrderBy(t => t.TicketNumber).ToList()
            };

            return tickets;
        }


    }
}
