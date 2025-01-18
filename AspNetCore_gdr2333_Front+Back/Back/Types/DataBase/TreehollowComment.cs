using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class TreehollowComment(long treehollowId, long senderId, string content)
    {
        [Key]
        public long CommentId { get; set; }

        public long TreehollowId { get; set; } = treehollowId;

        public long SenderId { get; set; } = senderId;

        public string Content { get; set; } = content;

        public DateTime SendTime { get; set; } = DateTime.Now;

        public bool Checked { get; set; } = false;

        public bool CheckSuccess { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public TreehollowComment() : this(0, 0, "")
        {
        }
    }
}
