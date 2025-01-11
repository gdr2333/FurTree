using System.ComponentModel.DataAnnotations;

namespace FurTreeFull.Data.DataBase;

public class Comment
{
    [Key]
    public long CommentId { get; set; }
    public required long MessageId { get; set; }
    public required string Content { get; set; }
    public DateTime SendTime { get; set; } = DateTime.Now;
}