using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services;
using NoSQLproject.Services.Interfaces;


namespace NoSQLproject.Controllers;

public class TicketController : Controller
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketService _ticketService;


    public TicketController(ITicketService ticketService, ITicketRepository ticketRepository)
    {
        _ticketService = ticketService;
        _ticketRepository = ticketRepository;
    }

    [HttpGet]
    public IActionResult Index(string searchEmail, string sortOrder)
    {
        var tickets = _ticketService.GetFilteredAndSortedTickets(searchEmail, sortOrder);

        ViewData["IdSortParam"] = sortOrder == "id_desc" ? "id_asc" : "id_desc";
        ViewData["UserSortParam"] = sortOrder == "user_desc" ? "user_asc" : "user_desc";
        ViewData["DateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";

        return View(tickets);
    }

    [HttpGet]
    public IActionResult CurrentIncidents(bool showList = false, string searchEmail = "", string sortOrder = "")
    {
        var tickets = _ticketService.GetFilteredAndSortedTickets(searchEmail, sortOrder);
        var openTickets = tickets
            .Where(t => t.State.ToString().Equals("Open", StringComparison.OrdinalIgnoreCase))
            .ToList();

        ViewBag.TotalTickets = tickets.Count;
        ViewBag.OpenTickets = openTickets.Count;
        ViewBag.ShowList = showList;

        ViewData["IdSortParam"] = sortOrder == "id_desc" ? "id_asc" : "id_desc";
        ViewData["UserSortParam"] = sortOrder == "user_desc" ? "user_asc" : "user_desc";
        ViewData["DateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";

        return View(openTickets);
    }



    [HttpGet]
    public IActionResult Create()
    {
        return View(); 
    }
    
    [HttpPost]
    public IActionResult Create(Ticket ticket)
    {
        Console.WriteLine("POST Create() called");

        // for testing, some required fields
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.CreatedBy = new User { FullName = "Test User" };
        ticket.HandledBy = new List<User>
        {
            new User { FullName = "Support Agent" }
        };;

        _ticketRepository.CreateTicket(ticket);

        Console.WriteLine("Ticket inserted");
        return RedirectToAction(nameof(Index));
    }

}