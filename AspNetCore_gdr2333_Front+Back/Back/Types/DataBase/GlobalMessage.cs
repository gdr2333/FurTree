using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class GlobalMessage(long senderId, string title, string content)
    {
        [Key]
        public long GlobalMessageId { get; set; }

        public long SenderId { get; set; } = senderId;

        public string Title { get; set; } = title;

        public string Content { get; set; } = content;

        public DateTime SendTime { get; set; } = DateTime.Now;

        public GlobalMessage() : this(0, "", "")
        {
        }
    }
}
