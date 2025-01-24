using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Back.Types.DataBase;

public class PrivateChat : DbContext
{
    [Key]
    public long ChatId { get; set; }

    public long User1 { get; set; }

    public long User2 { get; set; }

    public bool Replyed { get; set; }

    public DateTime LastMessageSendTime { get; set; }

    public long LastCheckedMessageId { get; set; }

    public PrivateChat(long user1, long user2)
    {
        User1 = user1;
        User2 = user2;
        Replyed = false;
        LastMessageSendTime = DateTime.Now;
        LastCheckedMessageId = -1;
    }

    public PrivateChat() { }
}