namespace Back.Types.Interface;

public interface IEmailSender
{
    Task SendEmail(string target, string subject, string content);
}
