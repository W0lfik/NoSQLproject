using Microsoft.AspNetCore.Mvc;
using NoSQLproject.Models;
using NoSQLproject.Models.Dto;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Controllers.Api;

[ApiController]
[Route("api/tickets")]
public class TicketsApiController : ControllerBase
{
    private readonly ITicketApiService _ticketApiService;

    public TicketsApiController(ITicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Ticket>> GetAll()
    {
        IEnumerable<Ticket> tickets = _ticketApiService.GetAll();
        return Ok(tickets);
    }

    [HttpGet("{ticketNumber:int}")]
    public ActionResult<Ticket> GetByTicketNumber(int ticketNumber)
    {
        Ticket? ticket = _ticketApiService.GetByTicketNumber(ticketNumber);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public ActionResult<Ticket> Create([FromBody] CreateTicketRequest request)
    {
        Ticket created = _ticketApiService.Create(request);
        return CreatedAtAction(nameof(GetByTicketNumber), new { ticketNumber = created.TicketNumber }, created);
    }

    [HttpPut("{ticketNumber:int}")]
    public ActionResult<Ticket> Update(int ticketNumber, [FromBody] UpdateTicketRequest request)
    {
        Ticket? updated = _ticketApiService.Update(ticketNumber, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{ticketNumber:int}")]
    public IActionResult Delete(int ticketNumber)
    {
        bool deleted = _ticketApiService.Delete(ticketNumber);
        return deleted ? NoContent() : NotFound();
    }
}
