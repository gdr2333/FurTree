using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class Treehollow
{
    [Key]
    public long TreehollowId { get; set; }

    public long SenderId { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public bool Checked { get; set; }

    public bool CheckSuccess { get; set; }

    public bool IsPublic { get; set; }

    public bool Deleted { get; set; }

    public Treehollow(long senderId, string content, bool isPublic)
    {
        SenderId = senderId;
        Content = content;
        IsPublic = isPublic;
        SendTime = DateTime.Now;
        Checked = false;
        CheckSuccess = false;
        Deleted = false;
    }

    public Treehollow()
    {
    }
}
