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
        private readonly TicketFilterService _filterService;


        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository,  IHttpContextAccessor httpContextAccessor, TicketFilterService filterService)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _filterService = filterService;
        }


        public List<Ticket> GetFilteredAndSortedTickets(string searchQuery, string sortOrder, User currentUser)
        {
            IEnumerable<Ticket> tickets = currentUser.TypeOfUser switch
            {
                TypeOfUser.employee => _ticketRepository.GetAllTickets()
                    .Where(t =>
                        (t.CreatedBy != null && t.CreatedBy.Id == currentUser.Id) ||
                        (t.HandledBy != null && t.HandledBy.Any(u => u.Id == currentUser.Id))),

                TypeOfUser.service_desk => _ticketRepository.GetAllTickets(),

                TypeOfUser.admin => _ticketRepository.GetAllTickets(),

                _ => Enumerable.Empty<Ticket>()
            };

            // Apply search + sort in the separate class
            tickets = _filterService.ApplySearchAndSort(tickets, searchQuery, sortOrder);

            return tickets.ToList();
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

            Ticket existing = _ticketRepository.GetTicketByNumber(vm.Ticket.TicketNumber);
            if (existing is null) return false;

            Priority? previousPriority = existing.Priority;
            State previousState = existing.State;

            existing.Title = vm.Ticket.Title;
            existing.Description = vm.Ticket.Description;
            existing.IncidentType = vm.Ticket.IncidentType;
            existing.Priority = vm.Ticket.Priority;
            existing.State = vm.Ticket.State;

            ApplyPeopleFromVm(existing, vm);
            existing.HandledBy ??= new List<UserInTicket>();

            bool priorityChanged = existing.Priority != previousPriority;
            if (priorityChanged)
            {
                if (existing.Priority.HasValue)
                {
                    existing.Deadline = existing.Priority.Value switch
                    {
                        Priority.P1 => existing.CreatedAt.AddDays(2),
                        Priority.P2 => existing.CreatedAt.AddDays(7),
                        Priority.P3 => existing.CreatedAt.AddDays(14),
                        Priority.P4 => existing.CreatedAt.AddDays(21),
                        _ => existing.Deadline
                    };
                }
                else
                {
                    existing.Deadline = null;
                }
            }

            bool stateChangedToResolved = existing.State == State.resolved && previousState != State.resolved;
            bool stateChangedFromResolved = existing.State != State.resolved && previousState == State.resolved;

            if (stateChangedToResolved)
            {
                existing.ResolvedAt = DateTime.UtcNow;
            }
            else if (stateChangedFromResolved)
            {
                existing.ResolvedAt = null;
            }
            else if (existing.State == State.resolved && existing.ResolvedAt is null)
            {
                existing.ResolvedAt = DateTime.UtcNow;
            }

            _ticketRepository.UpdateTicket(existing);
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
