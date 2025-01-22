using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class Post
{
    [Key]
    public long PostId { get; set; }

    public long SenderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public bool Checked { get; set; }

    public bool CheckSuccess { get; set; }

    public bool Deleted { get; set; }

    public Post(long senderId, string title, string content)
    {
        SenderId = senderId;
        Title = title;
        Content = content;
        SendTime = DateTime.Now;
        Checked = false;
        CheckSuccess = false;
        Deleted = false;
    }

    public Post()
    {
    }
}
