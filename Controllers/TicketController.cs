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
}