using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLproject.Models
{
    [BsonIgnoreExtraElements] // be resilient to missing/unexpected fields
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("employeeNumber")]
        public int EmployeeNumber { get; set; }

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;   // 

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;      // 

        [BsonElement("phone")]
        [BsonRepresentation(BsonType.String)]
        public string Phone { get; set; } = string.Empty;      // 

        [BsonElement("location")]
        public string Location { get; set; } = string.Empty;   // 

        [BsonElement("typeOfUser")]
        [BsonRepresentation(BsonType.String)]
        public TypeOfUser TypeOfUser { get; set; }

        public User() { }

        public User(string id, string fullName, int employeeNumber, string password,
                    string email, string phone, string location, TypeOfUser typeOfUser)
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
    }

   
}
