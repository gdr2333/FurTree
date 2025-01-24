namespace Shared.Results;
public class UncheckedTreehollowComment
{
    public required long CommentId { get; set; }
    public required long TreehollowId { get; set; }
    public required string Content { get; set; }
    public required long SenderId { get; set; }
    public required string SenderName { get; set; }
    public required DateTime SendTime { get; set; }
}
