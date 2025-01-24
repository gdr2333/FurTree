namespace Shared.Results;
public class UncheckedPost
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required long SenderId { get; set; }
    public required string SenderName { get; set; }
    public required DateTime SendTime { get; set; }
}
