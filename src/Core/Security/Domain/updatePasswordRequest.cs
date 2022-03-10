namespace Core.Domain;

public record struct UpdatePasswordRequest(string OldPassword,string NewPassword);
