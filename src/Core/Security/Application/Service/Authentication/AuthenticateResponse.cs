namespace Core.Domain;
public record struct AuthenticateResponse(string AccessToken, string RefreshToken,DateTime ValidTo);
