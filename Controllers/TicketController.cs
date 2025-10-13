using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;

namespace NoSQLproject.Controllers;

public class TicketController : Controller
{
    private readonly ITicketRepository _ticketRepository;
    
    public TicketController(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }
    
    public IActionResult Index()
    {
        List<Ticket> tickets = _ticketRepository.GetAllTickets();
        return View(tickets);
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