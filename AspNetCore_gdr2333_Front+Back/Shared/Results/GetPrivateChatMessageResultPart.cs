namespace Shared.Results;
public class GetPrivateChatMessageResultPart
{
    public required bool IsMyMessage { get; set; }
    public required long Id { get; set; }
    public required bool Checked { get; set; }
    public required bool CheckFail { get; set; }
    public required string Context { get; set; }
}
