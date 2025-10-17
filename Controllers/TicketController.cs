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
    public IActionResult Create() => View(_ticketService.BuildVmForCreate());

    [HttpPost]
    public IActionResult Create(CreateTicketViewModel vm)
    {
        if (!ModelState.IsValid) return View(_ticketService.BuildVmForCreate()); // or return View(vm) with repopulated UsersSelect
        _ticketService.CreateTicketFromVm(vm);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Ticket/Details/{ticketNumber:int}")]
    public IActionResult Details(int ticketNumber)
    {
        Ticket ticket = _ticketService.GetByTicketNumber(ticketNumber);
        return ticket is null ? NotFound() : View(ticket);
    }

    [HttpGet("Ticket/Edit/{ticketNumber:int}")]
    public IActionResult Edit(int ticketNumber)
    {
        CreateTicketViewModel vm = _ticketService.BuildVmForEdit(ticketNumber);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost]
    public IActionResult Edit(CreateTicketViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _ticketService.UpdateTicketFromVm(vm);
        return RedirectToAction(nameof(Details), new { ticketNumber = vm.Ticket.TicketNumber });
    }

    [HttpPost("Ticket/Delete/{ticketNumber:int}")]
    public IActionResult Delete(int ticketNumber)
    {
        _ticketService.DeleteByTicketNumber(ticketNumber);
        return RedirectToAction(nameof(Index));
    }



}