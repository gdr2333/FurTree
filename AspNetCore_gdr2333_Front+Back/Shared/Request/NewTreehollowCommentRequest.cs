namespace Shared.Request;

public class NewTreehollowCommentRequest
{
    public required long TreehollowId { get; set; }
    public required string Content { get; set; }
}
