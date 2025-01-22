namespace Shared.Request;

public class ResendConfirmRequest
{
    public required string Email { get; set; }
    public required string CapchaId { get; set; }
    public required string CapchaResult { get; set; }
}
