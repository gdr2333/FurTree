namespace Shared.Request;
public class SendPrivateMessageRequest
{
    public required long To { get; set; }
    public required string Content { get; set; }
}
