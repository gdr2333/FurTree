namespace Shared.Request;

public class CreateAccountRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string CapchaId { get; set; }
    public required string CapchaResult { get; set; }
}
