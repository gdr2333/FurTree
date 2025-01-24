namespace Shared.Request;
public class SetTreehollowPublicStatusRequest
{
    public required long Id { get; set; }
    public required bool IsPublic { get; set; }
}
