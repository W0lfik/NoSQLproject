using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;
using System.Security.Claims;

namespace NoSQLproject.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository,  IHttpContextAccessor httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
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
            Ticket ticket = vm.Ticket;
            ApplyPeopleFromVm(ticket, vm);

            // current user as CreatedBy
            ticket.CreatedBy = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeNumber") is Claim empNumClaim
                ? ToLightUser(_userRepository.GetByEmployeeNumber(int.Parse(empNumClaim.Value)))
                : null;

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
            vm.Ticket.HandledBy ??= new List<UserInTicket>();

            // Auto-assign deadline when managing based on priority
            Ticket existing = _ticketRepository.GetTicketByNumber(vm.Ticket.TicketNumber);
            if (existing is not null)
            {
                bool priorityChanged = vm.Ticket.Priority != existing.Priority;

                if (priorityChanged && vm.Ticket.Priority.HasValue)
                {
                    switch (vm.Ticket.Priority.Value)
                    {
                        case Priority.P1:
                            vm.Ticket.Deadline = existing.CreatedAt.AddDays(2);
                            break;
                        case Priority.P2:
                            vm.Ticket.Deadline = existing.CreatedAt.AddDays(7);
                            break;
                        case Priority.P3:
                            vm.Ticket.Deadline = existing.CreatedAt.AddDays(14);
                            break;
                        case Priority.P4:
                            vm.Ticket.Deadline = existing.CreatedAt.AddDays(21);
                            break;
                    }
                }
            }

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
                ticket.CreatedBy = ToLightUser(_userRepository.GetById(vm.CreatedByUserId));
            }

            // Assign HandledBy (multi)
            ticket.HandledBy = new List<UserInTicket>();

            if (vm.HandledByUserIds != null)
            {
                foreach (string id in vm.HandledByUserIds)
                {
                    User user = _userRepository.GetById(id);
                    if (user != null)
                        ticket.HandledBy.Add(ToLightUser(user));
                }
            }
        }

        private static UserInTicket ToLightUser(User u) => new UserInTicket(u);

    }
}
