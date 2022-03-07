namespace Presentation.Controllers;
public record struct Response(bool Success, string? Message = null, object? Data = null);
public static class ResponseFactory
{
    public static Response CreateSuccessResponse(object data)
    {
        return new Response() { Success = true, Data = data };
    }
    public static Response CreateErrorResponse(string message)
    {
        return new Response() { Success = false, Message = message };
    }
}