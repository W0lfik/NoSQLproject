using System.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

public class Ticket
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public int TicketNumber { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("incidentType")]
    [BsonRepresentation(BsonType.String)] 
    public IncedentType IncedentType { get; set; }
    
    [BsonElement("priority")]
    [BsonRepresentation(BsonType.String)] 
    public Priority Priority { get; set; }
    
    public DateTime Deadline { get; set; }
    
    public string Description { get; set; }
    
    [BsonElement("state")]
    [BsonRepresentation(BsonType.String)] 
    public State State { get; set; }
    
    public string Title { get; set; }
    
    public DateTime ResolvedAt { get; set; }
    
    public User CreatedBy { get; set; }
    
    public List<User> HandledBy { get; set; }
    
    public Ticket(string id, int ticketNumber, DateTime createdAt, IncedentType incedentType, Priority priority, DateTime deadline, string description, State state, string title, DateTime resolvedAt, User createdBy, List<User> handledBy)
    {
        Id = id;
        TicketNumber = ticketNumber;
        CreatedAt = createdAt;
        IncedentType = incedentType;
        Priority = priority;
        Deadline = deadline;
        Description = description;
        State = state;
        Title = title;
        ResolvedAt = resolvedAt;
        CreatedBy = createdBy;
        HandledBy = handledBy;
    }
    
    public Ticket() { }
}

