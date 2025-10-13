using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;

namespace NoSQLproject.Controllers;

public class TicketController : Controller
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    
    public TicketController(ITicketRepository ticketRepository, IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }
    
    public IActionResult Index()
    {
        List<Ticket> tickets = _ticketRepository.GetAllTickets();
        return View(tickets);
    }
    
    
    [HttpGet]
    public IActionResult Create()
    {
        List<User> users = _userRepository.GetAllUsers();
        
        var vm = new CreateTicketViewModel
        {
            UsersSelect = users.Select(user => new SelectListItem
            {
                Value = user.Id,
                Text  = $"{user.FullName} ({user.EmployeeNumber})"
            })
        };
        return View(vm);
    }
    
    [HttpPost]
    public IActionResult Create(CreateTicketViewModel vm)
    {
        Console.WriteLine("POST Create() called");

        if (!ModelState.IsValid)
        {
            // re-fill dropdowns on validation error
            var users = _userRepository.GetAllUsers();
            vm.UsersSelect = users.Select(u => new SelectListItem { Value = u.Id, Text = $"{u.FullName} ({u.EmployeeNumber})" });
            return View(vm);
        }

        var allUsers = _userRepository.GetAllUsers();

        // CreatedBy (optional)
        if (!string.IsNullOrWhiteSpace(vm.CreatedByUserId))
            vm.Ticket.CreatedBy = allUsers.FirstOrDefault(u => u.Id == vm.CreatedByUserId);

        // HandledBy (multi; can be empty)
        vm.Ticket.HandledBy = (vm.HandledByUserIds ?? new List<string>())
            .Select(id => allUsers.FirstOrDefault(u => u.Id == id))
            .Where(u => u != null)
            .ToList()!;

        // sensible defaults
        vm.Ticket.State = State.open;
        vm.Ticket.CreatedAt = DateTime.UtcNow;
        // vm.Ticket.ResolvedAt = default; // set only when resolved

        try
        {

            _ticketRepository.CreateTicket(vm.Ticket);

        }
        catch (Exception e)
        {
            Console.WriteLine("Ticket creation failed: "+e);
            throw;
        }
        Console.WriteLine("Ticket inserted");
        
        return RedirectToAction(nameof(Index));
    }


}