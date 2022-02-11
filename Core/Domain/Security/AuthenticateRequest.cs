namespace Core.Domain;
public record struct AuthenticateRequest(string UserName, string Password);