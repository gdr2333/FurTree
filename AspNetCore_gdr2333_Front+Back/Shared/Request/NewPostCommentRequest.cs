namespace Shared.Request;

public class NewPostCommentRequest
{
    public required long PostId { get; set; }
    public required string Content { get; set; }
}
