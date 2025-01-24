namespace Shared.Request;
public class SetAccountBannedToRequest
{
    public required long Id { get; set; }
    public required DateTime To { get; set; }
}
