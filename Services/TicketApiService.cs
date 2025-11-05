using NoSQLproject.Models;
using NoSQLproject.Models.Dto;
using NoSQLproject.Repositories.Interfaces;
using NoSQLproject.Services.Interfaces;

namespace NoSQLproject.Services;

public class TicketApiService : ITicketApiService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketApiService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public IEnumerable<Ticket> GetAll() => _ticketRepository.GetAllTickets();

    public Ticket? GetByTicketNumber(int ticketNumber) => _ticketRepository.GetTicketByNumber(ticketNumber);

    public Ticket Create(CreateTicketRequest request)
    {
        Ticket ticket = new()
        {
            Title = request.Title,
            Description = request.Description,
            IncidentType = request.IncidentType,
            Priority = request.Priority,
            HandledBy = MapHandlers(request.HandledBy)
        };

        if (request.CreatedBy is not null)
        {
            ticket.CreatedBy = request.CreatedBy.ToModel();
        }

        _ticketRepository.CreateTicket(ticket);

        return ticket;
    }

    public Ticket? Update(int ticketNumber, UpdateTicketRequest request)
    {
        Ticket? existing = _ticketRepository.GetTicketByNumber(ticketNumber);
        if (existing is null) return null;

        if (request.Title is not null)
        {
            existing.Title = request.Title;
        }

        if (request.Description is not null)
        {
            existing.Description = request.Description;
        }

        if (request.IncidentType.HasValue)
        {
            existing.IncidentType = request.IncidentType.Value;
        }

        if (request.Priority.HasValue)
        {
            existing.Priority = request.Priority.Value;
        }

        if (request.State.HasValue)
        {
            existing.State = request.State.Value;
        }

        if (request.Deadline.HasValue)
        {
            existing.Deadline = NormalizeToUtc(request.Deadline.Value);
        }

        if (request.ResolvedAt.HasValue)
        {
            existing.ResolvedAt = NormalizeToUtc(request.ResolvedAt.Value);
        }

        if (request.CreatedBy is not null)
        {
            existing.CreatedBy = request.CreatedBy.ToModel();
        }

        if (request.HandledBy is not null)
        {
            existing.HandledBy = MapHandlers(request.HandledBy);
        }

        _ticketRepository.UpdateTicket(existing);
        return existing;
    }

    public bool Delete(int ticketNumber)
    {
        Ticket? existing = _ticketRepository.GetTicketByNumber(ticketNumber);
        if (existing is null) return false;

        _ticketRepository.DeleteTicket(ticketNumber);
        return true;
    }

    private static List<UserInTicket> MapHandlers(IEnumerable<UserInTicketDto>? handlers)
    {
        if (handlers is null) return new List<UserInTicket>();

        List<UserInTicket> mapped = new();
        foreach (UserInTicketDto dto in handlers)
        {
            if (dto is not null)
            {
                mapped.Add(dto.ToModel());
            }
        }

        return mapped;
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
