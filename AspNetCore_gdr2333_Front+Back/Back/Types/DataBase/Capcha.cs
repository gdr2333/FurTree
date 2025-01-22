using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class Capcha
{
    [Key]
    public string Guid { get; set; }

    public string Result { get; set; }

    public DateTime DeleteAt { get; set; }

    public Capcha(string guid, string result)
    {
        Guid = guid;
        Result = result;
        DeleteAt = DateTime.Now.AddHours(1);
    }

    public Capcha()
    {
    }
}
