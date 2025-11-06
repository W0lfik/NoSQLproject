using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

[BsonIgnoreExtraElements]
public class UserInTicket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("fullName")]
    public required string FullName { get; set; }

    [BsonElement("employeeNumber")]
    public int EmployeeNumber { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("typeOfUser")]
    [BsonRepresentation(BsonType.String)]
    public TypeOfUser TypeOfUser { get; set; }

    public UserInTicket() { }

    [SetsRequiredMembers]
    public UserInTicket(User user)
    {
        Id = user.Id;
        FullName = user.FullName;
        EmployeeNumber = user.EmployeeNumber;
        Email = user.Email;
        TypeOfUser = user.TypeOfUser;
    }
}
