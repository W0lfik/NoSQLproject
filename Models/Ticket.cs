using System.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

public class Ticket
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    [BsonElement("ticketNumber")]
    public int TicketNumber { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("incidentType")]
    [BsonRepresentation(BsonType.String)] 
    public IncidentType IncidentType { get; set; }
    
    [BsonElement("priority")]
    [BsonRepresentation(BsonType.String)] 
    public Priority? Priority { get; set; }
    
    [BsonElement("deadline")]
    public DateTime? Deadline { get; set; }
    
    [BsonElement("description")]
    public string Description { get; set; }
    
    [BsonElement("state")]
    [BsonRepresentation(BsonType.String)] 
    public State State { get; set; }
    
    [BsonElement("title")]
    public string Title { get; set; }
    
    [BsonElement("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }
    
    [BsonElement("createdBy")]
    public UserInTicket CreatedBy { get; set; }
    
    [BsonElement("handledBy")]
    public List<UserInTicket> HandledBy { get; set; }
    
    public Ticket(string id, int ticketNumber, DateTime createdAt, IncidentType incidentType, Priority priority, DateTime deadline, string description, State state, string title, DateTime resolvedAt, UserInTicket createdBy, List<UserInTicket> handledBy)
    {
        Id = id;
        TicketNumber = ticketNumber;
        CreatedAt = createdAt;
        IncidentType = incidentType;
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

