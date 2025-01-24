namespace Shared.Request;
public class ForceSetPasswordRequest
{
    public required long Id { get; set; }
    public required string PasswordHash { get; set; }
}

