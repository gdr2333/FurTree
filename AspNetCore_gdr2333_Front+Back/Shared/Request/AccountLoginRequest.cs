namespace Shared.Request;

public class AccountLoginRequest
{
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public required string CapchaId { get; set; }
    public required string CapchaResult { get; set; }
}
