using Back.Types.DataBase;
using Back.Types.Interface;
using Back.Types.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Request;
using Shared.Results;

namespace Back.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AdminController(IDbContextFactory<MainDataBase> dbContextFactory, IEmailSender emailSender, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private readonly ILogger<AdminController> _logger = loggerFactory.CreateLogger<AdminController>();
    [Authorize]
    [HttpPut]
    public IActionResult SetAccountLockStatus(SetAccountLockStatusRequest request)
    {
        _logger.LogInformation($"开始更改账户锁定状态：账户：{request.Id}，目标：{request.Status}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Accounts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        target.Locked = request.Status;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpPut]

    public IActionResult SetAccountBannedTo(SetAccountBannedToRequest request)
    {
        _logger.LogInformation($"开始更改账户封禁时间：账户：{request.Id}，新的封禁到期日：{request.To}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Accounts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        target.BannedTo = request.To;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    private async Task<IActionResult?> SendConfirmEmailInternel(string email)
    {
        _logger.LogInformation($"开始发送确认邮件，邮箱：{email}");
        using var dbContext = dbContextFactory.CreateDbContext();
        if (!dbContext.Accounts.Any(account => account.Email == email))
        {
            _logger.LogWarning("发送确认邮件失败：未找到对应的邮箱");
            return NotFound();
        }
        var account = (from acc in dbContext.Accounts where acc.Email == email select acc).FirstOrDefault();
        if (account is null)
        {
            _logger.LogWarning("发送确认邮件失败：未找到对应的账户");
            return NotFound();
        }
        if (account.EmailConfired)
        {
            _logger.LogWarning("发送确认邮件失败：邮箱已被确认");
            return Conflict();
        }
        if (DateTime.Now - account.LastConfirmEmailSendTime <= new TimeSpan(1, 0, 0))
        {
            _logger.LogWarning("发送确认邮件失败：发送频率过高");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        if (account.Locked)
        {
            _logger.LogWarning("发送确认邮件失败：账户被锁定");
            return Forbid();
        }
        var confirmCode = new EmailConfirmCode(email, account.Id);
        dbContext.EmailConfirmCodes.Add(confirmCode);
        account.LastConfirmEmailSendTime = DateTime.Now;
        dbContext.SaveChanges();
        await emailSender.SendEmail(email, "账号确认", $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\" lang=\"zh-Hans\">\r\n<title>邮件确认</title>\r\n</head>\r\n<body>\r\n\r\n\t<a href=\"http://{Request.Host}/Account/Confirm?requestId={confirmCode.ConfirmCode}\">点击这里确认FurTree的账户创建确认邮件</a>\r\n\r\n</body>\r\n</html>");
        _logger.LogInformation("发送确认邮件成功");
        return null;
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> ForceSetEmail(ForceSetEmailRequest request)
    {
        _logger.LogInformation($"开始更改账户邮箱：账户：{request.Id}，新的邮箱：{request.Email}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Accounts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        target.Email = request.Email;
        var res = await SendConfirmEmailInternel(target.Email);
        if (res is not null)
        {
            _logger.LogWarning("确认邮箱时发生错误，返回错误码......");
            return res;
        }
        _logger.LogInformation("更改完成，返回204......");
        dbContext.SaveChanges();
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public IActionResult ForceSetName(ForceSetNameRequest request)
    {
        _logger.LogInformation($"开始更改用户名：账户：{request.Id}，新的用户名：{request.Name}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Accounts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        target.Name = request.Name;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public IActionResult ForceSetPassword(ForceSetPasswordRequest request)
    {
        _logger.LogInformation($"开始更改密码：账户：{request.Id}，新密码哈希值：{request.PasswordHash}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Accounts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标用户，返回404......");
            return NotFound();
        }
        target.PasswordHash = Convert.FromBase64String(request.PasswordHash);
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public IActionResult UncheckedPostList()
    {
        _logger.LogInformation($"开始获取未审查帖子列表");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from post in dbContext.Posts where !post.Checked orderby post.SendTime select post).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<UncheckedPost>());
        else
            return Ok(Array.ConvertAll(res, post => new UncheckedPost()
            {
                Content = post.Content,
                Id = post.PostId,
                SenderId = post.SenderId,
                SenderName = dbContext.Accounts.Find(post.SenderId).Name,
                SendTime = post.SendTime,
                Title = post.Title,
            }));
    }

    [Authorize]
    [HttpPut]
    public IActionResult CheckPost(CheckPostRequest request)
    {
        _logger.LogInformation($"开始审查帖子：Id：{request.Id}，审查结果：{request.Result}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Posts.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标帖子，返回404......");
            return NotFound();
        }
        target.Checked = true;
        target.CheckSuccess = request.Result;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public IActionResult UncheckedPostCommentList()
    {
        _logger.LogInformation($"开始获取未审查帖子评论列表");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from comment in dbContext.PostComments where !comment.Checked orderby comment.SendTime select comment).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<UncheckedPostComment>());
        else
            return Ok(Array.ConvertAll(res, comment => new UncheckedPostComment()
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                PostId = comment.PostId,
                SenderId = comment.SenderId,
                SenderName = dbContext.Accounts.Find(comment.SenderId).Name,
                SendTime = comment.SendTime,
            }));
    }

    [Authorize]
    [HttpPut]
    public IActionResult CheckPostComment(CheckPostCommentRequest request)
    {
        _logger.LogInformation($"开始审查帖子评论：Id：{request.Id}，审查结果：{request.Result}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.PostComments.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标帖子评论，返回404......");
            return NotFound();
        }
        target.Checked = true;
        target.CheckSuccess = request.Result;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public IActionResult UncheckedTreehollowList()
    {
        _logger.LogInformation($"开始获取未审查树洞列表");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from treehollow in dbContext.Treehollows where !treehollow.Checked orderby treehollow.SendTime select treehollow).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<UncheckedTreehollow>());
        else
            return Ok(Array.ConvertAll(res, treehollow => new UncheckedTreehollow()
            {
                Content = treehollow.Content,
                Id = treehollow.TreehollowId,
                SenderId = treehollow.SenderId,
                SenderName = dbContext.Accounts.Find(treehollow.SenderId).Name,
                SendTime = treehollow.SendTime,
            }));
    }

    [Authorize]
    [HttpPut]
    public IActionResult CheckTreehollow(CheckTreehollowRequest request)
    {
        _logger.LogInformation($"开始审查树洞：Id：{request.Id}，审查结果：{request.Result}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.Treehollows.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标树洞，返回404......");
            return NotFound();
        }
        target.Checked = true;
        target.CheckSuccess = request.Result;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public IActionResult UncheckedTreehollowCommentList()
    {
        _logger.LogInformation($"开始获取未审查树洞评论列表");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from comment in dbContext.TreehollowComments where !comment.Checked orderby comment.SendTime select comment).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<UncheckedTreehollowComment>());
        else
            return Ok(Array.ConvertAll(res, comment => new UncheckedTreehollowComment()
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                SenderId = comment.SenderId,
                SenderName = dbContext.Accounts.Find(comment.SenderId).Name,
                SendTime = comment.SendTime,
                TreehollowId = comment.TreehollowId,
            }));
    }

    [Authorize]
    [HttpPut]
    public IActionResult CheckTreehollowComment(CheckTreehollowCommentRequest request)
    {
        _logger.LogInformation($"开始审查树洞评论：Id：{request.Id}，审查结果：{request.Result}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.TreehollowComments.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标树洞评论，返回404......");
            return NotFound();
        }
        target.Checked = true;
        target.CheckSuccess = request.Result;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public IActionResult NewGlobalMessage(NewGlobalMessageRequest request)
    {
        _logger.LogInformation($"开始上传全局信息");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var globalMessage = new GlobalMessage(user.Id, request.Title, request.Content);
        dbContext.GlobalMessages.Add(globalMessage);
        dbContext.SaveChanges();
        return Ok(globalMessage.GlobalMessageId);
    }

    [Authorize]
    [HttpGet]
    public IActionResult UncheckedPrivateMessageList()
    {
        _logger.LogInformation($"开始获取未审查树洞评论列表");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from message in dbContext.PrivateMessages where !message.Checked orderby message.SendTime select message).ToArray();
        _logger.LogInformation("获取列表成功，返回200......");
        if (res is null)
            return Ok(Array.Empty<UncheckedPrivateMessage>());
        else
            return Ok(Array.ConvertAll(res, message => new UncheckedPrivateMessage()
            {
                Content = message.Content,
                FromId = message.SenderId,
                FromName = dbContext.Accounts.Find(message.SenderId).Name,
                MessageId = message.PrivateMessageId,
                ToId = message.ReceiverId,
                ToName = dbContext.Accounts.Find(message.ReceiverId).Name
            }));
    }

    [Authorize]
    [HttpPut]
    public IActionResult CheckPrivateMessage(CheckPrivateMessageRequest request)
    {
        _logger.LogInformation($"开始审查私信：Id：{request.Id}，审查结果：{request.Result}");
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("无法获取用户信息，返回401......");
            return Unauthorized();
        }
        if (!user.IsAdmin)
        {
            _logger.LogWarning("用户不是管理员，返回403......");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var target = dbContext.PrivateMessages.Find(request.Id);
        if (target is null)
        {
            _logger.LogWarning("找不到目标私信，返回404......");
            return NotFound();
        }
        target.Checked = true;
        target.CheckSuccess = request.Result;
        dbContext.SaveChanges();
        _logger.LogInformation("更改完成，返回204......");
        return NoContent();
    }
}