using Back.Types.DataBase;
using Back.Types.Utils;
using Baidu.Aip.ContentCensor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Request;
using Shared.Results;
using static Back.Types.Utils.StaticValues;

namespace Back.Controllers;
public class PrivateMessageController(IDbContextFactory<MainDataBase> dbContextFactory, TextCensor baiduAi, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private readonly ILogger<PrivateMessageController> _logger = loggerFactory.CreateLogger<PrivateMessageController>();
    [Authorize]
    [HttpPut]
    public IActionResult Send(SendPrivateMessageRequest request)
    {
        _logger.LogInformation("开始发送私聊消息");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("用户无法识别，返回401......");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("用户被封禁或锁定，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var to = dbContext.Accounts.Find(request.To);
        if (to is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        var chat = (from ch in dbContext.PrivateChats where ch.User1 == user.Id && ch.User2 == request.To select ch).FirstOrDefault();
        if (chat is null)
        {
            _logger.LogInformation("建立新聊天......");
            chat = new PrivateChat(user.Id, to.Id);
            dbContext.PrivateChats.Add(chat);
            dbContext.SaveChanges();
        }
        if (!chat.Replyed)
        {
            if (user.ThisDayUnresivedPrivateMessageSend > PerDayUnresivedPrivateMessageSendMax)
            {
                _logger.LogWarning("达到限制次数，返回429......");
                return StatusCode(StatusCodes.Status429TooManyRequests);
            }
        }
        var privatMessage = new PrivateMessage(chat.ChatId, user.Id, to.Id, request.Content);
        dbContext.PrivateMessages.Add(privatMessage);
        dbContext.SaveChanges();
        var checkResult = baiduAi.TextCensorUserDefined(privatMessage.Content)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：{checkResult}");
        if (checkResult == "合规")
        {
            privatMessage.Checked = true;
            privatMessage.CheckSuccess = true;
            chat.LastMessageSendTime = privatMessage.SendTime;
            chat.LastCheckedMessageId = privatMessage.PrivateMessageId;
            dbContext.SaveChanges();
            _logger.LogInformation("自动审查成功，返回204......");
            return NoContent();
        }
        else
        {
            _logger.LogInformation("进入手动审查队列，返回202......");
            return Accepted();
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult UserList()
    {
        var user = GetUserFromJwt();
        _logger.LogInformation($"开始获取私聊消息列表：用户Id：{user?.Id}");
        if (user is null)
        {
            _logger.LogWarning("用户无法识别，返回401......");
            return Unauthorized();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from chat in dbContext.PrivateChats where chat.User1 == user.Id || chat.User2 == user.Id orderby chat.LastMessageSendTime descending select chat).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<PrivateChatUserListPart>());
        else
            return Ok(Array.ConvertAll(res, chat => new PrivateChatUserListPart()
            {
                LastMessage = dbContext.PrivateMessages.Find(chat.LastCheckedMessageId)?.Content ?? "",
                UserId = user.Id,
                UserName = dbContext.Accounts.Find(chat.User1 == user.Id ? chat.User2 : chat.User1).Name
            }));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ToUser([FromQuery] long Id)
    {
        var user = GetUserFromJwt();
        _logger.LogInformation($"开始获取私聊消息列表：当前用户Id：{user?.Id}，目标用户Id：{Id}");
        if (user is null)
        {
            _logger.LogWarning("用户无法识别，返回401......");
            return Unauthorized();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var chat = (from ch in dbContext.PrivateChats where (ch.User1 == user.Id && ch.User2 == Id) || (ch.User1 == Id && ch.User2 == user.Id) select ch).FirstOrDefault();
        if(chat is null)
        {
            _logger.LogWarning("找不到指定聊天");
            return NotFound();
        }
        var messages = (from msg in dbContext.PrivateMessages where msg.PrivateChatId == chat.ChatId && (msg.SenderId == user.Id || msg.CheckSuccess) orderby msg.SendTime descending select msg).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (messages is null)
            return Ok(Array.Empty<GetPrivateChatMessageResultPart>());
        else
            return Ok(Array.ConvertAll(messages, msg => new GetPrivateChatMessageResultPart()
            {
                Checked = msg.Checked,
                CheckFail = !msg.CheckSuccess,
                Context = msg.Content,
                Id = msg.PrivateMessageId,
                IsMyMessage = msg.SenderId == user.Id
            }));
    }
}
