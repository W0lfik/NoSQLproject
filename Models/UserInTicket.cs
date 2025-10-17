using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models;

[BsonIgnoreExtraElements] 
public class UserInTicket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; }

    [BsonElement("employeeNumber")]
    public int EmployeeNumber { get; set; }
    
    [BsonElement("email")]
    public string Email { get; set; }
    
    [BsonElement("typeOfUser")]
    [BsonRepresentation(BsonType.String)] 
    public TypeOfUser TypeOfUser { get; set; }
    
    public UserInTicket(User user)
    {
        Id = user.Id;
        FullName = user.FullName;
        EmployeeNumber = user.EmployeeNumber;
        Email = user.Email;
        TypeOfUser = user.TypeOfUser;
    }
}