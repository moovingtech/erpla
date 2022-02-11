namespace Core.Domain;
public record struct AddUserRequest(string FirstName, string LastName, string UserName, string Email, string Password, IList<string> Roles);