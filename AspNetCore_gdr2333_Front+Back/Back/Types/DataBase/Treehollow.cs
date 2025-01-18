using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class Treehollow(long senderId, string content, bool isPublic)
    {
        [Key]
        public long TreehollowId { get; set; }

        public long SenderId { get; set; } = senderId;

        public string Content { get; set; } = content;

        public DateTime SendTime { get; set; } = DateTime.Now;

        public bool Checked { get; set; } = false;

        public bool CheckSuccess { get; set; } = false;

        public bool IsPublic { get; set; } = isPublic;

        public bool Deleted { get; set; } = false;

        public Treehollow() : this(0, "", true)
        {
        }
    }
}
