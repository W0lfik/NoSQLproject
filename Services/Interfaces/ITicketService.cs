using NoSQLproject.Models;

namespace NoSQLproject.Services.Interfaces
{
    public interface ITicketService
    {
        List<Ticket> GetFilteredAndSortedTickets(string searchEmail, string sortOrder);
        
        Ticket? GetByTicketNumber(int ticketNumber);
        CreateTicketViewModel BuildVmForCreate();
        void CreateTicketFromVm(CreateTicketViewModel vm);
        CreateTicketViewModel? BuildVmForEdit(int ticketNumber);
        bool UpdateTicketFromVm(CreateTicketViewModel vm);
        void DeleteByTicketNumber(int ticketNumber);
    }
}