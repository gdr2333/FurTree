namespace Shared.Results;
public class PrivateChatUserListPart
{
    public required long UserId { get; set; }
    public required string UserName { get; set; }
    public required string LastMessage { get; set; }
}
