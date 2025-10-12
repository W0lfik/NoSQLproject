using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; }

    [BsonElement("employeeNumber")]
    public int EmployeeNumber { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("phone")]
    [BsonRepresentation(BsonType.String)]
    public string Phone { get; set; }

    [BsonElement("location")]
    public string Location { get; set; }

    [BsonElement("typeOfUser")]
    [BsonRepresentation(BsonType.String)] 
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