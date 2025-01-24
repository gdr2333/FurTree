namespace Shared.Results;
public class GetGlobalMessageIndexResult
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string SenderName { get; set; }
    public required long SenderId { get; set; }
    public required DateTime SendTime { get; set; }
}