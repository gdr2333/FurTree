using System.ComponentModel.DataAnnotations;

namespace FurTree.Types.DataBase;

public class Comment
{
    [Key]
    public long CommentId { get; set; }
    public required long MessageId { get; set; }
    public required string Context { get; set; }
}