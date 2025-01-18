namespace Shared.Results;
public class GetCapchaResult
{
    public string Image { get; set; }
    public string Id { get; set; }
    public GetCapchaResult() { }
    public GetCapchaResult(string id, string image)
    {
        Id = id;
        Image = image;
    }
}