using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Back.Types.DataBase
{
    public class EmailConfirmCode
    {
        [Key]
        public string ConfirmCode { get; set; }

        public string ToEmail { get; set; }

        public DateTime DeleteAt { get; set; }

        public EmailConfirmCode(string toEmail)
        {
            var buffer = new byte[15];
            ToEmail = toEmail;
            RandomNumberGenerator.Fill(buffer);
            ConfirmCode = Convert.ToBase64String(buffer);
            DeleteAt = DateTime.Now.AddDays(1);
        }

        public EmailConfirmCode() : this("")
        {
        }
    }
}
