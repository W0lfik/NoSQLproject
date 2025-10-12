using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } 
    
    public string FullName { get; set; } 
    
    public int EmployeeNumber { get; set; }
    
    public string Password { get; set; }
    
    public string Email { get; set; }
    
    public string Phone { get; set; }
    
    public string Location { get; set; }
    
    [BsonElement("typeOfUser")]
    public TypeOfUser TypeOfUser { get; set; }
    
    public User(string id, string fullName, int employeeNumber, string password, string email, string phone, string location, TypeOfUser typeOfUser)
    {
        Id = id;
        FullName = fullName;
        EmployeeNumber = employeeNumber;
        Password = password;
        Email = email;
        Phone = phone;
        Location = location;
        TypeOfUser = typeOfUser;
    }
    
    public User() { }
}