namespace Shared.Request;
public class SetAccountLockStatusRequest
{
    public required long Id { get; set; }
    public required bool Status { get; set; }
}
