using Microsoft.AspNetCore.Components;

namespace FurTreeFull.Utils;

public static class HintEmailSender
{
    public static Task SendNewCommentHint(long messageId, string messageSender, NavigationManager navigationManager) =>
        EmailSender.SendEmailAsync(messageSender, "你发表的树洞有新的回复",
        $"<html lang=\"zh-hans\"><head></head><body>你发表的<a href={navigationManager.ToAbsoluteUri($"/Message/{messageId}")}>这个树洞</a>有新的回复。</body></html>");
}