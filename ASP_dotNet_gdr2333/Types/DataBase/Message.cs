using System.ComponentModel.DataAnnotations;

namespace FurTree.Types.DataBase;

public class Message
{
    [Key]
    public long Id { get; set; } = 0;
    public required string Content { get; set; }
}