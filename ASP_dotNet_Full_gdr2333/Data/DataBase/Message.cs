using System.ComponentModel.DataAnnotations;

namespace FurTreeFull.Data.DataBase;

public class Message
{
    [Key]
    public long Id { get; set; }
    public required string Sender { get; set; }
    public required string Content { get; set; }
    public DateTime SendTime { get; set; } = DateTime.Now;
}