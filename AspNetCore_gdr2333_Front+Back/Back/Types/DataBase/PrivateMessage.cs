using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class PrivateMessage
{
    [Key]
    public long PrivateMessageId { get; set; }

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public bool Checked { get; set; }

    public bool CheckSuccess { get; set; }

    public bool Readed { get; set; }

    public PrivateMessage(long senderId, long receiverId, string content)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        SendTime = DateTime.Now;
        Checked = false;
        CheckSuccess = false;
        Readed = false;
    }

    public PrivateMessage()
    {
    }
}
