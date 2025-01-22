namespace Shared.Results;

public class GetPostIndexResult
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string SenderName { get; set; }
    public required long SenderId { get; set; }
    public required DateTime SendTime { get; set; }
    public required PostComment[] Comments { get; set; }
}

public class PostComment
{
    public required string Content { get; set; }
    public required string SenderName { get; set; }
    public required long SenderId { get; set; }
    public required DateTime SendTime { get; set; }
}
