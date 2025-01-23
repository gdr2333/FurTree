using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class TreehollowComment
{
    [Key]
    public long CommentId { get; set; }

    public long TreehollowId { get; set; }

    public long SenderId { get; set; }

    public string Content { get; set; }

    public DateTime SendTime { get; set; }

    public bool Checked { get; set; }

    public bool CheckSuccess { get; set; }

    public bool Deleted { get; set; }

    public TreehollowComment(long treehollowId, long senderId, string content)
    {
        TreehollowId = treehollowId;
        SenderId = senderId;
        Content = content;
        SendTime = DateTime.Now;
        Checked = false;
        CheckSuccess = false;
        Deleted = false;
    }

    public TreehollowComment()
    {
    }
}
