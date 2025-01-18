using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase
{
    public class Capcha(string guid, byte[] result)
    {
        [Key]
        public string Guid { get; set; } = guid;

        public byte[] Result { get; set; } = result;

        public DateTime DeleteAt { get; set; } = DateTime.Now.AddHours(1);

        public Capcha() : this("", [])
        {
        }
    }
}
