using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
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

                "user_desc" => tickets.OrderByDescending(t => t.CreatedBy?.FullName).ToList(),
                "user_asc" => tickets.OrderBy(t => t.CreatedBy?.FullName).ToList(),

                "date_desc" => tickets.OrderByDescending(t => t.CreatedAt).ToList(),
                "date_asc" => tickets.OrderBy(t => t.CreatedAt).ToList(),

                "deadline_desc" => tickets.OrderByDescending(t => t.Deadline).ToList(),
                "deadline_asc" => tickets.OrderBy(t => t.Deadline).ToList(),

                _ => tickets.OrderBy(t => t.TicketNumber).ToList()
            };


            return tickets;
        }

        public Ticket? GetByTicketNumber(int ticketNumber) =>
            _ticketRepository.GetTicketByNumber(ticketNumber);
        
        public CreateTicketViewModel BuildVmForCreate()
        {
            List<User> all = _userRepository.GetAllUsers();
            return new CreateTicketViewModel
            {
                UsersSelect = all.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.EmployeeNumber})"
                })
            };
        }

        public void CreateTicketFromVm(CreateTicketViewModel vm)
        {
            Ticket ticket = vm.Ticket ?? new Ticket();
            ApplyPeopleFromVm(ticket, vm);

            // defaults
            ticket.State = State.open;
            ticket.CreatedAt = DateTime.UtcNow;

            _ticketRepository.CreateTicket(ticket); // your repo does auto-numbering
        }
        
        public CreateTicketViewModel? BuildVmForEdit(int ticketNumber)
        {
            Ticket ticket = _ticketRepository.GetTicketByNumber(ticketNumber);
            if (ticket is null) return null;

            List<User> all = _userRepository.GetAllUsers();
            return new CreateTicketViewModel
            {
                Ticket = ticket,
                CreatedByUserId = ticket.CreatedBy?.Id,
                HandledByUserIds = ticket.HandledBy?.Select(u => u.Id).ToList() ?? new List<string>(),
                UsersSelect = all.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.EmployeeNumber})"
                })
            };
        }

        public bool UpdateTicketFromVm(CreateTicketViewModel vm)
        {
            if (vm.Ticket is null) return false;

            ApplyPeopleFromVm(vm.Ticket, vm);

            // keep CreatedAt as-is
            vm.Ticket.HandledBy ??= new List<User>();

            _ticketRepository.UpdateTicket(vm.Ticket);
            return true;
        }
        
        public void DeleteByTicketNumber(int ticketNumber) =>
            _ticketRepository.DeleteTicket(ticketNumber);

        // ---------- Helpers ----------
        private void ApplyPeopleFromVm(Ticket ticket, CreateTicketViewModel vm)
        {
            // Assign CreatedBy (single)
            if (!string.IsNullOrWhiteSpace(vm.CreatedByUserId))
            {
                ticket.CreatedBy = _userRepository.GetById(vm.CreatedByUserId);
            }

            // Assign HandledBy (multi)
            ticket.HandledBy = new List<User>();

            if (vm.HandledByUserIds != null)
            {
                foreach (string id in vm.HandledByUserIds)
                {
                    User user = _userRepository.GetById(id);
                    if (user != null)
                        ticket.HandledBy.Add(user);
                }
            }
        }
        
    }
}
