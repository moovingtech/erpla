using Microsoft.AspNetCore.Identity;

namespace Core.Domain;

public class User : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}