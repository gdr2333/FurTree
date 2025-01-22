using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Back.Types.DataBase;

public class EmailConfirmCode
{
    [Key]
    public string ConfirmCode { get; set; }

    public string ToEmail { get; set; }

    public long AccountId { get; set; }

    public DateTime DeleteAt { get; set; }

    public EmailConfirmCode(string toEmail, long accountId)
    {
        ToEmail = toEmail;
        ConfirmCode = Guid.NewGuid().ToString();
        DeleteAt = DateTime.Now.AddDays(1);
        AccountId = accountId;
    }

    public EmailConfirmCode()
    {
    }
}
