using NoSQLproject.Models;
using NoSQLproject.Models.Dto;

namespace NoSQLproject.Services.Interfaces;

public interface ITicketApiService
{
    IEnumerable<Ticket> GetAll();
    Ticket? GetByTicketNumber(int ticketNumber);
    Ticket Create(CreateTicketRequest request);
    Ticket? Update(int ticketNumber, UpdateTicketRequest request);
    bool Delete(int ticketNumber);
}
