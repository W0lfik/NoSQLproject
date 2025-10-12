using NoSQLproject.Models;

namespace NoSQLproject.Repositories.Interfaces;

public interface ITicketRepository
{
    List<Ticket> GetAllTickets();
    Ticket? GetTicketByNumber(int ticketNumber);
    void CreateTicket(Ticket ticket);
    void UpdateTicket(Ticket ticket);
    void DeleteTicket(int ticketNumber);
}