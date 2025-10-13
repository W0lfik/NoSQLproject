using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services;
using NoSQLproject.Services.Interfaces;


namespace NoSQLproject.Controllers;

public class TicketController : Controller
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketService _ticketService;


    public TicketController(ITicketService ticketService, ITicketRepository ticketRepository, IUserRepository userRepository)
    {
        _ticketService = ticketService;
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Index(string searchEmail, string sortOrder)
    {
        var tickets = _ticketService.GetFilteredAndSortedTickets(searchEmail, sortOrder);

        ViewData["IdSortParam"] = sortOrder == "id_desc" ? "id_asc" : "id_desc";
        ViewData["UserSortParam"] = sortOrder == "user_desc" ? "user_asc" : "user_desc";
        ViewData["DateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
        ViewData["DeadlineSortParam"] = sortOrder == "deadline_desc" ? "deadline_asc" : "deadline_desc";


        return View(tickets);
    }

    [HttpGet]
    public IActionResult CurrentIncidents(bool showList = false, string searchEmail = "", string sortOrder = "", string viewType = "open")
    {
        var tickets = _ticketService.GetFilteredAndSortedTickets(searchEmail, sortOrder);

        var openTickets = tickets
            .Where(t => t.State.ToString().Equals("Open", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var overdueTickets = openTickets
            .Where(t => t.Deadline < DateTime.UtcNow)
            .ToList();

        var model = viewType == "overdue" ? overdueTickets : openTickets;

        ViewBag.TotalTickets = tickets.Count;
        ViewBag.OpenTickets = openTickets.Count;
        ViewBag.OverdueTickets = overdueTickets.Count;
        ViewBag.ShowList = showList;
        ViewBag.ViewType = viewType;

        ViewData["IdSortParam"] = sortOrder == "id_desc" ? "id_asc" : "id_desc";
        ViewData["UserSortParam"] = sortOrder == "user_desc" ? "user_asc" : "user_desc";
        ViewData["DateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
        ViewData["DeadlineSortParam"] = sortOrder == "deadline_desc" ? "deadline_asc" : "deadline_desc";


        return View(model);
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

        // defaults
        vm.Ticket.State = State.open;
        vm.Ticket.CreatedAt = DateTime.UtcNow;
        

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
    
    [HttpGet("Ticket/Details/{ticketNumber:int}")]
    public IActionResult Details(int ticketNumber)
    {
        var ticket = _ticketRepository.GetTicketByNumber(ticketNumber);
        if (ticket is null) return NotFound();
        return View(ticket); 
    }
    
    // GET: /Ticket/Edit/400
[HttpGet("Ticket/Edit/{ticketNumber:int}")]
public IActionResult Edit(int ticketNumber)
{
    var ticket = _ticketRepository.GetTicketByNumber(ticketNumber);
    if (ticket is null) return NotFound();

    // load users for dropdowns
    var users = _userRepository.GetAllUsers();
    var vm = new CreateTicketViewModel
    {
        Ticket = ticket,
        CreatedByUserId = ticket.CreatedBy?.Id,
        HandledByUserIds = ticket.HandledBy?.Select(u => u.Id).ToList() ?? new List<string>(),
        UsersSelect = users.Select(u => new SelectListItem
        {
            Value = u.Id,
            Text  = $"{u.FullName} ({u.EmployeeNumber})"
        })
    };

    return View(vm); // Views/Ticket/Edit.cshtml (same VM as Create)
}

// POST: /Ticket/Edit
[HttpPost]
public IActionResult Edit(CreateTicketViewModel vm)
{
    // repopulate selects if validation fails
    if (!ModelState.IsValid)
    {
        var users = _userRepository.GetAllUsers();
        vm.UsersSelect = users.Select(u => new SelectListItem
        {
            Value = u.Id,
            Text  = $"{u.FullName} ({u.EmployeeNumber})"
        });
        return View(vm);
    }

    // map selected users back to embedded objects
    var allUsers = _userRepository.GetAllUsers();

    vm.Ticket.CreatedBy = string.IsNullOrWhiteSpace(vm.CreatedByUserId)
        ? null
        : allUsers.FirstOrDefault(u => u.Id == vm.CreatedByUserId);

    vm.Ticket.HandledBy = (vm.HandledByUserIds ?? new List<string>())
        .Select(id => allUsers.FirstOrDefault(u => u.Id == id))
        .Where(u => u != null)
        .ToList()!;

    // do NOT overwrite CreatedAt; keep the original value
    vm.Ticket.HandledBy ??= new List<User>();

    _ticketRepository.UpdateTicket(vm.Ticket);  // updates by TicketNumber in your repo
    return RedirectToAction(nameof(Details), new { ticketNumber = vm.Ticket.TicketNumber });
}

// POST: /Ticket/Delete/400
[HttpPost("Ticket/Delete/{ticketNumber:int}")]
public IActionResult Delete(int ticketNumber)
{
    _ticketRepository.DeleteTicket(ticketNumber);
    return RedirectToAction(nameof(Index));
}



}