using System.Collections;

namespace NoSQLproject.Models;

public class Ticket
{
    public string id { get; set; }
    public int ticketNumber { get; set; }
    public DateTime createdAt { get; set; }
    public IncedentType incedentType { get; set; }
    public Priority priority { get; set; }
    public DateTime deadline { get; set; }
    public string description { get; set; }
    public State state { get; set; }
    public string title { get; set; }
    public DateTime resolvedAt { get; set; }
    public User createdBy { get; set; }
    public List<User> handledBy { get; set; }
    
    public Ticket(string id, int ticketNumber, DateTime createdAt, IncedentType incedentType, Priority priority, DateTime deadline, string description, State state, string title, DateTime resolvedAt, User createdBy, List<User> handledBy)
    {
        this.id = id;
        this.ticketNumber = ticketNumber;
        this.createdAt = createdAt;
        this.incedentType = incedentType;
        this.priority = priority;
        this.deadline = deadline;
        this.description = description;
        this.state = state;
        this.title = title;
        this.resolvedAt = resolvedAt;
        this.createdBy = createdBy;
        this.handledBy = handledBy;
    }
    
    public Ticket() { }
}

