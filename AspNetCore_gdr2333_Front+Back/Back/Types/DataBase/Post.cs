using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class Post(long senderId, string title, string content)
    {
        [Key]
        public long PostId { get; set; }

        public long SenderId { get; set; } = senderId;

        public string Title { get; set; } = title;

        public string Content { get; set; } = content;

        public DateTime SendTime { get; set; } = DateTime.Now;

        public bool Checked { get; set; } = false;

        public bool CheckSuccess { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public Post() : this(0, "", "")
        {
        }
    }
}
