namespace Shared.Results;

public class GetTreehollowIndexResult
{
    public required string Content { get; set; }
    public required DateTime SendTime { get; set; }
    public required TreehollowComment[] Comments { get; set; }
}

public class TreehollowComment
{
    public required string Content { get; set; }
    public required DateTime SendTime { get; set; }
}
