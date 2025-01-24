namespace Shared.Results;
public class UncheckedPrivateMessage
{
    public required long MessageId { get; set; }
    public required string FromName { get; set; }
    public required long FromId { get; set; }
    public required string ToName { get; set; }
    public required long ToId { get; set; }
    public required string Content { get; set; }
}
