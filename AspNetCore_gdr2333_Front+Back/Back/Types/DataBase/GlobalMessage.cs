using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class GlobalMessage
{
    [Key]
    public long GlobalMessageId { get; set; }

    public long SenderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public GlobalMessage(long senderId, string title, string content)
    {
        SenderId = senderId;
        Title = title;
        Content = content;
        SendTime = DateTime.Now;
    }

    public GlobalMessage()
    {
    }
}
