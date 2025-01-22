using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class PostComment
{
    [Key]
    public long CommentId { get; set; }

    public long PostId { get; set; }

    public long SenderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public bool Checked { get; set; }

    public bool CheckSuccess { get; set; }

    public bool Deleted { get; set; }

    public PostComment(long postId, long senderId, string title, string content)
    {
        PostId = postId;
        SenderId = senderId;
        Title = title;
        Content = content;
        SendTime = DateTime.Now;
        Checked = false;
        CheckSuccess = false;
        Deleted = false;
    }

    public PostComment()
    {
    }
}
