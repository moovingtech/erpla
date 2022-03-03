namespace Presentation.Controllers;
public record struct Response(bool Success, string? Message = null, object? Data = null);
