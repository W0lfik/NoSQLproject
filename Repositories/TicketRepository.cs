using MongoDB.Driver;
using NoSQLproject.Models;
using NoSQLproject.Repositories.Interfaces;

namespace NoSQLproject.Repositories;

public class TicketRepository : ITicketRepository
{
    private static MongoClient? _client;
    private readonly IMongoCollection<Ticket> _ticket;
    
    public TicketRepository(IConfiguration config)
    {
        _client ??= new MongoClient(config["MongoDbSettings:ConnectionString"]);
        IMongoDatabase database = _client.GetDatabase(config["MongoDbSettings:DatabaseName"]);

        // access the "ticket" collection
        _ticket = database.GetCollection<Ticket>("ticket");
    }

    public List<Ticket> GetAllTickets()
    {
        return _ticket.Find(ticket => true).ToList();
    }

    public Ticket? GetTicketByNumber(int ticketNumber)
    {
        return _ticket.Find(t => t.TicketNumber == ticketNumber).FirstOrDefault();
    }

    public void CreateTicket(Ticket ticket)
    {
        _ticket.InsertOne(ticket);
    }

    public void UpdateTicket(Ticket ticket)
    {
        _ticket.ReplaceOne(t => t.TicketNumber == ticket.TicketNumber, ticket);
    }

    public void DeleteTicket(int ticketNumber)
    {
        _ticket.DeleteOne(t => t.TicketNumber == ticketNumber);
    }
}