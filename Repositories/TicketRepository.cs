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
        try
        {
            _ticket.InsertOne(ticket);
            Console.WriteLine("Insert successful!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Mongo insert error: " + ex.Message);
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


}