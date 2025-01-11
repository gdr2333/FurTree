using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

namespace FurTreeFull.Utils;

public static class EmailSender
{
    public static async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var messageToSend = new MimeMessage()
        {
            Sender = new MailboxAddress("MailNickname", "Email"),
            Subject = subject,
            Body = new TextPart(TextFormat.Html)
            {
                Text = message
            }
        };
        messageToSend.From.Add(new MailboxAddress("MailNickname", "Email"));
        messageToSend.To.Add(new MailboxAddress(toEmail, toEmail));
        using var smtp = new SmtpClient();
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await smtp.ConnectAsync("SMTP_Server");
        await smtp.AuthenticateAsync("Email", "token_password");
        await smtp.SendAsync(messageToSend);
        await smtp.DisconnectAsync(true);
    }
}