namespace FurTree.Types.Request;

public class SendCommentRequest
{
    public required long MessageId { get; set; }
    public required string Content { get; set; }
}