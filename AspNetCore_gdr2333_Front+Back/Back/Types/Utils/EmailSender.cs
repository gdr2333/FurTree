using Back.Types.Interface;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Back.Types.Utils
{
    public class EmailSender(IConfiguration config) : IEmailSender
    {
        public async Task SendEmail(string target, string subject, string content)
        {
            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(config["EmailConfirm:SmtpAddress"], 587, false);

                var message = new MimeMessage
                {
                    Sender = new MailboxAddress(config["EmailConfirm:SmtpAccount"], config["EmailConfirm:SmtpAccount"]),
                    Subject = subject
                };
                message.From.Add(new MailboxAddress(config["EmailConfirm:SmtpAccount"], config["EmailConfirm:SmtpAccount"]));

                var textPart = new TextPart(TextFormat.Plain)
                {
                    Text = content
                };

                message.Body = textPart;
                message.To.Add(new MailboxAddress(target, target));

                await smtpClient.AuthenticateAsync(config["EmailConfirm:SmtpAccount"], config["EmailConfirm:SmtpPassword"]);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
