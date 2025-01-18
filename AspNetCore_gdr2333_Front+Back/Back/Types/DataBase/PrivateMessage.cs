using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class PrivateMessage(long senderId, long receiverId, string content)
    {
        [Key]
        public long PrivateMessageId { get; set; }

        public long SenderId { get; set; } = senderId;

        public long ReceiverId { get; set; } = receiverId;

        public string Content { get; set; } = content;

        public DateTime SendTime { get; set; } = DateTime.Now;

        public bool Checked { get; set; } = false;

        public bool CheckSuccess { get; set; } = false;

        public PrivateMessage() : this(0, 0, "")
        {
        }
    }
}
