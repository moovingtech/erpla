using Microsoft.AspNetCore.Identity;

namespace Core.Security.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = default!;
    
    public string LastName { get; set; } = default!;
    
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public bool IsDisabled { get; set; }
}