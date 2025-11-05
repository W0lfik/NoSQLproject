using MongoDB.Bson;
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
        try
        {

            _client ??= new MongoClient(config["MongoDbSettings:ConnectionString"]);
            IMongoDatabase database = _client.GetDatabase(config["MongoDbSettings:DatabaseName"]);

            // access the "ticket" collection
            _ticket = database.GetCollection<Ticket>("ticket");
            
            var keys = Builders<Ticket>.IndexKeys.Ascending(t => t.TicketNumber);
            _ticket.Indexes.CreateOne(
                new CreateIndexModel<Ticket>(keys, new CreateIndexOptions { Unique = true })
            );

        }
        catch (Exception e)
        {
            Console.WriteLine("db connection failed: "+e);
            throw;
        }
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
        // 1) server-side defaults
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.State = State.open;
        ticket.ResolvedAt = null;
        ticket.HandledBy ??= new List<UserInTicket>();

        // 2) assign TicketNumber if missing/zero
        if (ticket.TicketNumber <= 0)
        {
            ticket.TicketNumber = AutoNumber();
        }

        // 3) insert; retry once if dup (race)
        try
        {
            _ticket.InsertOne(ticket);
            Console.WriteLine("Insert successful!");
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // someone inserted the same number just before us → recompute and retry once
            ticket.TicketNumber = AutoNumber();

            _ticket.InsertOne(ticket);
            Console.WriteLine("Insert successful after retry!");
        }
    }

    public void UpdateTicket(Ticket ticket)
    {
        _ticket.ReplaceOne(t => t.TicketNumber == ticket.TicketNumber, ticket);
    }

    public void DeleteTicket(int ticketNumber)
    {
        _ticket.DeleteOne(t => t.TicketNumber == ticketNumber);
    }

    public Ticket? GetById(string id)
    {
        return _ticket.Find(t => t.Id == id).FirstOrDefault();
    }

    private int AutoNumber()
    {
        var last = _ticket.Find(FilterDefinition<Ticket>.Empty)
            .SortByDescending(t => t.TicketNumber)
            .Limit(1)
            .FirstOrDefault();
        
        return (last?.TicketNumber ?? 0) + 1;
    }
    
}
