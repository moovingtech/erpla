namespace Core.Domain;

public record struct UpdatePasswordRequest(string UserName,string OldPassword,string NewPassword);
