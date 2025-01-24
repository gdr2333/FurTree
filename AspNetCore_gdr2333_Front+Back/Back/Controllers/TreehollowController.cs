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
public class TreehollowController(IDbContextFactory<MainDataBase> dbContextFactory, TextCensor baiduAi, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private readonly ILogger<TreehollowController> _logger = loggerFactory.CreateLogger<TreehollowController>();
    [Authorize]
    [HttpPut]
    public IActionResult New([FromBody] string context)
    {
        _logger.LogInformation($"开始执行树洞上传：内容：{context}");
        var user = GetUserFromJwt();
        if(string.IsNullOrWhiteSpace(context))
        {
            _logger.LogWarning("内容无效，返回400......");
            return BadRequest();
        }
        if(user is null)
        {
            _logger.LogWarning("无法识别的用户，返回401......");
            return Unauthorized();
        }
        if(UserLockedOrBanned(user))
        {
            _logger.LogWarning("用户被封禁或锁定，返回403......");
            return Forbid();
        }
        if (user.ThisHourTreehollowSend > PerHourTreehollowSendMax || user.ThisDayTreehollowSend > PerDayTreehollowSendMax)
        {
            _logger.LogWarning("发送次数到达上限，返回429......");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        user.ThisHourTreehollowSend++;
        user.ThisDayTreehollowSend++;
        var treehollow = new Treehollow(user.Id, context, true);
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Treehollows.Add(treehollow);
        dbContext.SaveChanges();
        var checkResult = baiduAi.TextCensorUserDefined(context)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：{checkResult}");
        if (checkResult == "合规")
        {
            _logger.LogInformation("树洞发送成功：状态：自动审查成功");
            treehollow.Checked = true;
            treehollow.CheckSuccess = true;
            dbContext.SaveChanges();
            return Ok(treehollow.TreehollowId);
        }
        else
        {
            _logger.LogInformation("树洞发送成功：状态：等待手动审查");
            return Accepted(treehollow.TreehollowId);
        }
    }

    [Authorize]
    [HttpPut]
    public IActionResult NewNotPublic([FromBody] string context)
    {
        _logger.LogInformation($"开始执行树洞上传：内容：{context}");
        var user = GetUserFromJwt();
        if (string.IsNullOrWhiteSpace(context))
        {
            _logger.LogWarning("内容无效，返回400......");
            return BadRequest();
        }
        if (user is null)
        {
            _logger.LogWarning("无法识别的用户，返回401......");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("用户被封禁或锁定，返回403......");
            return Forbid();
        }
        if (user.ThisHourTreehollowSend > PerHourTreehollowSendMax || user.ThisDayTreehollowSend > PerDayTreehollowSendMax)
        {
            _logger.LogWarning("发送次数到达上限，返回429......");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        user.ThisHourTreehollowSend++;
        user.ThisDayTreehollowSend++;
        var treehollow = new Treehollow(user.Id, context, false);
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Treehollows.Add(treehollow);
        dbContext.SaveChanges();
        var checkResult = baiduAi.TextCensorUserDefined(context)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：{checkResult}");
        if (checkResult == "合规")
        {
            _logger.LogInformation("树洞发送成功：状态：自动审查成功");
            treehollow.Checked = true;
            treehollow.CheckSuccess = true;
            dbContext.SaveChanges();
            return Ok(treehollow.TreehollowId);
        }
        else
        {
            _logger.LogInformation("树洞发送成功：状态：等待手动审查");
            return Accepted(treehollow.TreehollowId);
        }
    }

    [Authorize]
    [HttpPut]
    public IActionResult PublicStatus(SetTreehollowPublicStatusRequest request)
    {
        _logger.LogInformation($"开始更改树洞可见性：目标树洞：{request.Id}");
        var user = GetUserFromJwt();
        if(user is null)
        {
            _logger.LogWarning("无法识别的用户，返回401......");
            return Unauthorized();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var treehollow = dbContext.Treehollows.Find(request.Id);
        if(treehollow is null)
        {
            _logger.LogWarning("找不到指定树洞，返回404......");
            return NotFound();
        }
        if(UserLockedOrBanned(user) || (treehollow.SenderId != user.Id && !user.IsAdmin))
        {
            _logger.LogWarning("越权访问，返回403......");
            return Forbid();
        }
        treehollow.IsPublic = request.IsPublic;
        dbContext.SaveChanges();
        _logger.LogInformation($"更改成功：当前可见性：{treehollow.IsPublic}");
        return NoContent();
    }

    [HttpGet]
    public IActionResult Status([FromQuery] long Id)
    {
        _logger.LogInformation($"开始查询树洞状态：树洞ID：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var treehollow = dbContext.Treehollows.Find(Id);
        if (treehollow is null || treehollow.Deleted)
        {
            _logger.LogWarning($"查询树洞状态失败：树洞不存在或已被删除，树洞ID：{Id}");
            return NotFound();
        }
        else
        {
            int status = treehollow.Checked ? treehollow.CheckSuccess ? 1 : 2 : 0;
            _logger.LogInformation($"查询树洞状态成功：树洞ID：{Id}，状态：{status}");
            return Ok(status);
        }
    }

    [Authorize]
    [HttpPut]
    public IActionResult NewComment(NewTreehollowCommentRequest request)
    {
        _logger.LogInformation($"开始发布评论流程：树洞ID：{request.TreehollowId}，评论内容：{request.Content}");
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            _logger.LogWarning("发布评论失败：评论内容为空");
            return BadRequest();
        }
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("发布评论失败：无法获取用户信息");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("发布评论失败：用户被封禁或锁定");
            return Forbid();
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var treehollow = dbContext.Treehollows.Find(request.TreehollowId);
        if (treehollow is null || treehollow.Deleted)
        {
            _logger.LogWarning($"发布评论失败：树洞不存在或已被删除，树洞ID：{request.TreehollowId}");
            return NotFound();
        }
        if (user.ThisHourTreehollowCommentSend > PerHourTreehollowCommentSendMax || user.ThisDayTreehollowCommentSend > PerDayTreehollowCommentSendMax)
        {
            _logger.LogWarning("发布评论失败：发送次数到达上限");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        user.ThisHourTreehollowCommentSend++;
        user.ThisDayTreehollowCommentSend++;
        var comment = new Types.DataBase.TreehollowComment(request.TreehollowId, user.Id, request.Content);
        dbContext.TreehollowComments.Add(comment);
        dbContext.SaveChanges();

        var checkResult = baiduAi.TextCensorUserDefined(request.Content)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：评论ID：{comment.CommentId}，审查结论：{checkResult}");
        if (checkResult == "合规")
        {
            comment.Checked = true;
            comment.CheckSuccess = true;
            dbContext.SaveChanges();
            _logger.LogInformation($"评论发布查成功：状态：自动审查成功");
            return Ok(treehollow.TreehollowId);
        }
        else
        {
            _logger.LogInformation($"评论发布查成功：状态：等待手动审查");
            return Accepted(comment.CommentId);
        }
    }

    [HttpGet]
    public IActionResult CommentStatus([FromQuery] long Id)
    {
        _logger.LogInformation($"查询评论状态：评论ID：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var comment = dbContext.TreehollowComments.Find(Id);
        if (comment is null || comment.Deleted)
        {
            _logger.LogWarning($"查询评论状态失败：评论不存在或已被删除，评论ID：{Id}");
            return NotFound();
        }
        else
        {
            int status = comment.Checked ? (comment.CheckSuccess ? 1 : 2) : 0;
            _logger.LogInformation($"查询评论状态成功：评论ID：{Id}，状态：{status}");
            return Ok(status);
        }
    }

    [HttpGet]
    public IActionResult Index([FromQuery] long Id)
    {
        _logger.LogInformation($"开始获取树洞内容：Id：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var treehollow = dbContext.Treehollows.Find(Id);
        var user = GetUserFromJwt();
        if (treehollow is null || (treehollow.Deleted && !(user?.IsAdmin ?? false)))
        {
            _logger.LogWarning("树洞不存在或已被删除");
            return NotFound();
        }
        if (!treehollow.CheckSuccess && !(user?.IsAdmin ?? false))
        {
            _logger.LogWarning("树洞审查未完成或未通过");
            return StatusCode(StatusCodes.Status451UnavailableForLegalReasons);
        }
        var comments = (from comment in dbContext.TreehollowComments
                        where comment.CheckSuccess && !comment.Deleted && comment.TreehollowId == Id
                        orderby comment.SendTime descending
                        select comment).ToArray();
        var commentsResult = Array.ConvertAll(comments, comment => new Shared.Results.TreehollowComment()
        {
            Content = comment.Content,
            SendTime = comment.SendTime
        });
        var result = new GetTreehollowIndexResult()
        {
            Comments = commentsResult,
            Content = treehollow.Content,
            SendTime = treehollow.SendTime,
        };
        _logger.LogInformation("树洞内容获取成功");
        return Ok(result);
    }

    [HttpGet]
    public IActionResult All()
    {
        _logger.LogInformation("开始获取树洞列表");
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from treehollow in dbContext.Treehollows
                   where treehollow.Checked && !treehollow.Deleted
                   orderby treehollow.SendTime descending
                   select treehollow).ToArray();
        _logger.LogInformation("树洞列表获取成功");
        return Ok(Array.ConvertAll(res, treehollow => new GetAllTreehollowPart()
        {
            CommentNuber = dbContext.TreehollowComments.Where(comment => comment.TreehollowId == treehollow.TreehollowId && comment.CheckSuccess && !comment.Deleted).Count(),
            Id = treehollow.TreehollowId,
            SendTime = treehollow.SendTime,
        }));
    }

    [Authorize]
    [HttpDelete]
    public IActionResult Delete([FromQuery] long Id)
    {
        _logger.LogInformation($"开始删除树洞：Id：{Id}");
        var user = GetUserFromJwt();
        using var dbContext = dbContextFactory.CreateDbContext();
        var treehollow = dbContext.Treehollows.Find(Id);
        if (treehollow is null)
        {
            _logger.LogWarning("树洞不存在");
            return NotFound();
        }
        if (user is null)
        {
            _logger.LogWarning("JWT无效");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user) || (treehollow.SenderId != user.Id && !user.IsAdmin))
        {
            _logger.LogWarning("用户越权访问");
            return Forbid();
        }
        treehollow.Deleted = true;
        dbContext.SaveChanges();
        _logger.LogInformation("删除成功");
        return Ok();
    }

    [Authorize]
    [HttpDelete]
    public IActionResult DeleteComment([FromQuery] long Id)
    {
        _logger.LogInformation($"开始删除树洞：Id：{Id}");
        var user = GetUserFromJwt();
        using var dbContext = dbContextFactory.CreateDbContext();
        var comment = dbContext.TreehollowComments.Find(Id);
        if (comment is null)
        {
            _logger.LogWarning("树洞不存在");
            return NotFound();
        }
        if (user is null)
        {
            _logger.LogWarning("JWT无效");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user) || (comment.SenderId != user.Id && !user.IsAdmin))
        {
            _logger.LogWarning("用户越权访问");
            return Forbid();
        }
        comment.Deleted = true;
        dbContext.SaveChanges();
        _logger.LogInformation("删除成功");
        return Ok();
    }

    [Authorize]
    [HttpGet]
    public IActionResult Manageable()
    {
        var user = GetUserFromJwt();
        _logger.LogInformation($"开始获取可管理树洞列表：用户：{user?.Name}，管理员状态：{user?.IsAdmin}");
        if (user is null)
        {
            _logger.LogWarning("找不到用户信息，返回401......");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("用户被封禁或锁定，返回403......");
            return Forbid();
        }
        if (user.IsAdmin)
        {
            _logger.LogInformation("用户是管理员，返回所有树洞......");
            using var dbContext = dbContextFactory.CreateDbContext();
            var allTreehollow = dbContext.Treehollows.Where(treehollow => !treehollow.Deleted).ToArray();
            return Ok(Array.ConvertAll(allTreehollow, treehollow => new ManageableTreehollowPart()
            {
                Id = treehollow.TreehollowId,
                SenderId = treehollow.SenderId,
                SenderName = dbContext.Accounts.Find(treehollow.SenderId).Name,
                SendTime = treehollow.SendTime,
            }));
        }
        else
        {
            _logger.LogInformation("用户不是管理员，返回该用户发布的树洞......");
            using var dbContext = dbContextFactory.CreateDbContext();
            var allTreehollow = dbContext.Treehollows.Where(treehollow => !treehollow.Deleted && treehollow.SenderId == user.Id).ToArray();
            return Ok(Array.ConvertAll(allTreehollow, treehollow => new ManageableTreehollowPart()
            {
                Id = treehollow.TreehollowId,
                SenderId = treehollow.SenderId,
                SenderName = dbContext.Accounts.Find(treehollow.SenderId).Name,
                SendTime = treehollow.SendTime,
            }));
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult ManageableComment()
    {
        var user = GetUserFromJwt();
        _logger.LogInformation($"开始获取可管理树洞评论列表：用户：{user?.Name}，管理员状态：{user?.IsAdmin}");
        if (user is null)
        {
            _logger.LogWarning("找不到用户信息，返回401......");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("用户被封禁或锁定，返回403......");
            return Forbid();
        }
        if (user.IsAdmin)
        {
            _logger.LogInformation("用户是管理员，返回所有树洞评论......");
            using var dbContext = dbContextFactory.CreateDbContext();
            var allComment = dbContext.TreehollowComments.Where(comment => !comment.Deleted).ToArray();
            return Ok(Array.ConvertAll(allComment, comment => new ManageableTreehollowCommentPart()
            {
                TreehollowId = comment.TreehollowId,
                CommentId = comment.CommentId,
                Content = comment.Content,
                SenderId = comment.SenderId,
                SenderName = dbContext.Accounts.Find(comment.SenderId).Name,
                SendTime = comment.SendTime,
            }));
        }
        else
        {
            _logger.LogInformation("用户不是管理员，返回该用户发布的树洞......");
            using var dbContext = dbContextFactory.CreateDbContext();
            var allTreehollow = dbContext.TreehollowComments.Where(comment => !comment.Deleted && comment.SenderId == user.Id).ToArray();
            return Ok(Array.ConvertAll(allTreehollow, treehollow => new ManageableTreehollowCommentPart()
            {
                TreehollowId = treehollow.TreehollowId,
                CommentId = treehollow.CommentId,
                Content = treehollow.Content,
                SenderId = treehollow.SenderId,
                SenderName = dbContext.Accounts.Find(treehollow.SenderId).Name,
                SendTime = treehollow.SendTime,
            }));
        }
    }
}