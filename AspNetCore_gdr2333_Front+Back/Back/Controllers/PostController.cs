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
public class PostController(IDbContextFactory<MainDataBase> dbContextFactory, TextCensor baiduAi, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private ILogger<PostController> _logger = loggerFactory.CreateLogger<PostController>();
    [Authorize]
    [HttpPut]
    public IActionResult New(NewPostRequest request)
    {
        _logger.LogInformation($"开始发布帖子流程：标题：{request.Title}，内容：{request.Content}");
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            _logger.LogWarning("发布失败：标题或内容为空");
            return BadRequest();
        }
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("发布失败：无法获取用户信息");
            return Unauthorized();
        }
        if (UserLockedOrBanned(user))
        {
            _logger.LogWarning("发布失败：用户被封禁或锁定");
            return Forbid();
        }
        if (user.ThisHourPostSend > PerHourPostSendMax || user.ThisDayPostSend > PerDayPostSendMax)
        {
            _logger.LogWarning("发布失败：发送次数到达上限");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        user.ThisHourPostSend++;
        user.ThisDayPostSend++;
        var post = new Post(user.Id, request.Title, request.Content);
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Posts.Add(post);
        dbContext.SaveChanges();
        var titleCheckResult = baiduAi.TextCensorUserDefined(request.Title)["conclusion"].ToString();
        var contentCheckResult = baiduAi.TextCensorUserDefined(request.Content)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：标题：{titleCheckResult}，内容：{contentCheckResult}");
        if (titleCheckResult == "合规" && contentCheckResult == "合规")
        {
            post.Checked = true;
            post.CheckSuccess = true;
            dbContext.SaveChanges();
            return Ok(post.PostId);
        }
        else
        {
            _logger.LogInformation("帖子发送成功：状态：等待手动审查");
            return Accepted(post.PostId);
        }
    }

    [HttpGet]
    public IActionResult Status([FromQuery] long Id)
    {
        _logger.LogInformation($"开始查询帖子状态：帖子ID：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var post = dbContext.Posts.Find(Id);
        if (post is null || post.Deleted)
        {
            _logger.LogWarning($"查询帖子状态失败：帖子不存在或已被删除，帖子ID：{Id}");
            return NotFound();
        }
        else
        {
            int status = post.Checked ? post.CheckSuccess ? 1 : 2 : 0;
            _logger.LogInformation($"查询帖子状态成功：帖子ID：{Id}，状态：{status}");
            return Ok(status);
        }
    }

    [Authorize]
    [HttpPut]
    public IActionResult NewComment(NewPostCommentRequest request)
    {
        _logger.LogInformation($"开始发布评论流程：帖子ID：{request.PostId}，评论内容：{request.Content}");
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
        var post = dbContext.Posts.Find(request.PostId);
        if (post is null || post.Deleted)
        {
            _logger.LogWarning($"发布评论失败：帖子不存在或已被删除，帖子ID：{request.PostId}");
            return NotFound();
        }
        if (user.ThisHourPostCommentSend > PerHourPostCommentSendMax || user.ThisDayPostCommentSend > PerDayPostCommentSendMax)
        {
            _logger.LogWarning("发布评论失败：发送次数到达上限");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
        user.ThisHourPostCommentSend++;
        user.ThisDayPostCommentSend++;
        var comment = new Types.DataBase.PostComment(request.PostId, user.Id, request.Content);
        dbContext.PostComments.Add(comment);
        dbContext.SaveChanges();

        var checkResult = baiduAi.TextCensorUserDefined(request.Content)["conclusion"].ToString();
        _logger.LogInformation($"自动审查结果：评论ID：{comment.CommentId}，审查结论：{checkResult}");
        if (checkResult == "合规")
        {
            comment.Checked = true;
            comment.CheckSuccess = true;
            dbContext.SaveChanges();
            _logger.LogInformation($"评发布查成功：状态：自动审查成功");
            return Ok(post.PostId);
        }
        else
        {
            _logger.LogInformation($"评发布查成功：状态：等待手动审查");
            return Accepted(comment.CommentId);
        }
    }

    [HttpGet]
    public IActionResult CommentStatus([FromQuery] long Id)
    {
        _logger.LogInformation($"查询评论状态：评论ID：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var comment = dbContext.PostComments.Find(Id);
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
        _logger.LogInformation($"开始获取帖子内容：Id：{Id}");
        using var dbContext = dbContextFactory.CreateDbContext();
        var post = dbContext.Posts.Find(Id);
        if (post is null || post.Deleted)
        {
            _logger.LogWarning("帖子不存在或已被删除");
            return NotFound();
        }
        if (!post.CheckSuccess)
        {
            _logger.LogWarning("帖子审查未完成或未通过");
            return StatusCode(StatusCodes.Status451UnavailableForLegalReasons);
        }
        var comments = (from comment in dbContext.PostComments
                        where comment.CheckSuccess && !comment.Deleted && comment.PostId == Id
                        orderby comment.SendTime descending
                        select comment).ToArray();
        var commentsResult = Array.ConvertAll(comments, comment => new Shared.Results.PostComment()
        {
            Content = comment.Content,
            SenderId = comment.SenderId,
            SenderName = dbContext.Accounts.Find(comment.SenderId).Name,
            SendTime = comment.SendTime
        });
        var result = new GetPostIndexResult()
        {
            Comments = commentsResult,
            Content = post.Content,
            SenderId = post.SenderId,
            SendTime = post.SendTime,
            Title = post.Title,
            SenderName = dbContext.Accounts.Find(post.SenderId).Name
        };
        _logger.LogWarning("帖子内容获取成功");
        return Ok(result);
    }

    [HttpGet]
    public IActionResult All()
    {
        _logger.LogInformation("开始获取帖子列表");
        using var dbContext = dbContextFactory.CreateDbContext();
        var res = (from post in dbContext.Posts
                   where post.Checked && !post.Deleted
                   orderby post.SendTime descending
                   select post).ToArray();
        _logger.LogInformation("帖子列表获取成功");
        return Ok(Array.ConvertAll(res, post => new GetAllPostPart()
        {
            CommentNuber = dbContext.PostComments.Where(comment => comment.PostId == post.PostId && comment.CheckSuccess && !comment.Deleted).Count(),
            Id = post.PostId,
            SenderId = post.SenderId,
            SenderName = dbContext.Accounts.Find(post.SenderId).Name,
            SendTime = post.SendTime,
            Title = post.Title,
        }));
    }
}