namespace NoSQLproject.Models;

public class User
{
    public string id { get; set; } 
    public string fullName { get; set; } 
    public int employeeNumber { get; set; }
    public string password { get; set; }
    public string email { get; set; }
    public int phone { get; set; }
    public string location { get; set; }
    public TypeOfUser typeOfUser { get; set; }
    
    public User(string id, string fullName, int employeeNumber, string password, string email, int phone, string location, TypeOfUser typeOfUser)
    {
        this.id = id;
        this.fullName = fullName;
        this.employeeNumber = employeeNumber;
        this.password = password;
        this.email = email;
        this.phone = phone;
        this.location = location;
        this.typeOfUser = typeOfUser;
    }
    
    public User() { }
}