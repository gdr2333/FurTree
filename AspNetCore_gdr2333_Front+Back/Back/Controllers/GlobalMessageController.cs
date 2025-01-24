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

[Route("[controller]/[action]")]
[ApiController]
public class GlobalMessageController(IDbContextFactory<MainDataBase> dbContextFactory, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private readonly ILogger<GlobalMessageController> _logger = loggerFactory.CreateLogger<GlobalMessageController>();

    [Authorize]
    [HttpGet]
    public IActionResult Check()
    {
        var user = GetUserFromJwt();
        _logger.LogInformation($"开始检查未读全局消息");
        if(user is null)
        {
            _logger.LogWarning($"无法获取用户信息，返回401......");
            return Unauthorized();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var lastmessage = dbContext.GlobalMessages.LastOrDefault();
        _logger.LogInformation("返回200......");
        if (lastmessage is null || lastmessage.SendTime <= user.LastGlobalMessageReadTime)
            return Ok(false);
        else
            return Ok(true);
    }

    [HttpGet]
    public IActionResult All()
    {
        _logger.LogInformation("开始获取全局消息列表");
        using var dbContext = dbContextFactory.CreateDbContext();
        var globalMessages = dbContext.GlobalMessages.ToArray();
        _logger.LogInformation($"全局消息获取完成，共{globalMessages.Length}条，正在返回200......");
        return Ok(Array.ConvertAll(globalMessages, message => new GetAllGlobalMessagePart()
        {
            Id = message.GlobalMessageId,
            SenderId = message.SenderId,
            SenderName = dbContext.Accounts.Find(message.SenderId).Name,
            SendTime = message.SendTime,
            Title = message.Title,
        }));
    }

    [HttpGet]
    public IActionResult Index([FromQuery] long Id)
    {
        _logger.LogInformation($"开始获取指定全局消息：Id：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var message = dbContext.GlobalMessages.Find(Id);
        if (message is null)
        {
            _logger.LogWarning("找不到指定的全局消息，返回404......");
            return NotFound();
        }
        else
        {
            _logger.LogInformation("正在返回200......");
            return Ok(new GetGlobalMessageIndexResult()
            {
                Content = message.Content,
                SenderId = message.SenderId,
                SenderName = dbContext.Accounts.Find(message.SenderId).Name,
                SendTime = message.SendTime,
                Title = message.Title,
            });
        }
    }
}