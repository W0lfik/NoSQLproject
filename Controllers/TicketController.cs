using System.Linq;
using System.Security.Claims;
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
    private readonly ITicketArchiveService _ticketArchiveService;


    public TicketController(ITicketService ticketService, ITicketRepository ticketRepository, IUserRepository userRepository, ITicketArchiveService ticketArchiveService)
    {
        _ticketService = ticketService;
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _ticketArchiveService = ticketArchiveService;
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

    [HttpGet]
    public IActionResult Manage(int ticketNumber)
    {
        CreateTicketViewModel vm = _ticketService.BuildVmForEdit(ticketNumber);
        return vm is null ? NotFound() : View(vm);
    }
    
    [HttpPost]
    public IActionResult Manage(CreateTicketViewModel vm)
    {
        // 1. Check if the model state is valid
        if (!ModelState.IsValid)
        {
            var tempVm = _ticketService.BuildVmForEdit(vm.Ticket.TicketNumber);
            if (tempVm != null)
            {
                // Transfer back the user input and return the view
                tempVm.Ticket = vm.Ticket;
                tempVm.CreatedByUserId = vm.CreatedByUserId;
                tempVm.HandledByUserIds = vm.HandledByUserIds;
                return View(tempVm);
            }
            return View(vm); 
        }
       
        _ticketService.UpdateTicketFromVm(vm);
        
        // 3. Redirect to the Details view after a successful save
        return RedirectToAction(nameof(Details), new { ticketNumber = vm.Ticket.TicketNumber });
    }

    [HttpGet]
    public IActionResult SearchHandlerByEmployeeNumber(int employeeNumber)
    {
        if (employeeNumber <= 0)
        {
            return Json(new { found = false, message = "Enter a valid employee number." });
        }

        User user = _userRepository.GetByEmployeeNumber(employeeNumber);
        if (user is null)
        {
            return Json(new { found = false, message = $"No employee with number {employeeNumber}." });
        }

        return Json(new
        {
            found = true,
            user = new
            {
                id = user.Id,
                fullName = user.FullName,
                employeeNumber = user.EmployeeNumber,
                typeOfUser = user.TypeOfUser.ToString()
            }
        });
    }

    [HttpGet]
    public IActionResult SearchHandlersByRole(TypeOfUser role)
    {
        List<User> users = _userRepository.GetUsersByRole(role) ?? new List<User>();
        if (!users.Any())
        {
            return Json(new
            {
                found = false,
                message = $"No handlers with role {role}."
            });
        }

        return Json(new
        {
            found = true,
            role = role.ToString(),
            users = users.Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                employeeNumber = u.EmployeeNumber,
                typeOfUser = u.TypeOfUser.ToString()
            })
        });
    }



    // Anastasia's individual part 
    [HttpPost]
    public IActionResult ArchiveOldTickets()
    {
        var count = _ticketArchiveService.ArchiveOldTickets();

        if (count > 0)
        {
            TempData["MessageType"] = "success";
            TempData["Message"] = $"{count} ticket(s) older than 1 year have been archived successfully.";
        }
        else
        {
            TempData["MessageType"] = "info";
            TempData["Message"] = "No tickets older than 1 year were found to archive.";
        }

        return RedirectToAction("Index");
    }


}
