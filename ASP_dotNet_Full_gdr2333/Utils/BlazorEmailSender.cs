using FurTreeFull.Data;
using Microsoft.AspNetCore.Identity;

namespace FurTreeFull.Utils;

public class BlazorEmailSender(ILogger<BlazorEmailSender> logger) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email,
        string confirmationLink) => EmailSender.SendEmailAsync(email, "确认邮箱",
        "<html lang=\"zh-hans\"><head></head><body>请确认你的邮箱：" +
        $"<a href='{confirmationLink}'>点击这里</a>。</body></html>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email,
        string resetLink) => EmailSender.SendEmailAsync(email, "重置密码",
        "<html lang=\"zh-hans\"><head></head><body>重置密码： " +
        $"<a href='{resetLink}'>点击这里</a>。</body></html>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email,
        string resetCode) => EmailSender.SendEmailAsync(email, "重置密码",
        "<html lang=\"zh-hans\"><head></head><body>重置密码代码： " +
        $"<br>{resetCode}</body></html>");
}