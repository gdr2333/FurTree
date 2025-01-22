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
public class PostController(IDbContextFactory<MainDataBase> dbContextFactory, JwtHelper jwtHelper, TextCensor baiduAi) : ControllerBaseEx(dbContextFactory)
{
    [Authorize]
    [HttpPut]
    public IActionResult New(NewPostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest();
        var user = GetUserFromJwt();
        if (user is null)
            return Unauthorized();
        if (UserLockedOrBanned(user))
            return Forbid();
        if (user.ThisHourPostSend > PerHourPostSendMax || user.ThisDayPostSend > PerDayPostSendMax)
            return StatusCode(StatusCodes.Status429TooManyRequests);
        user.ThisHourPostSend++;
        user.ThisDayPostSend++;
        var post = new Post(user.Id, request.Title, request.Content);
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Posts.Add(post);
        dbContext.SaveChanges();
        var titleCheckResult = baiduAi.TextCensorUserDefined(request.Title)["conclusion"].ToString();
        var contentCheckResult = baiduAi.TextCensorUserDefined(request.Content)["conclusion"].ToString();
        if (titleCheckResult == "合规" && contentCheckResult == "合规")
        {
            post.Checked = true;
            post.CheckSuccess = true;
            dbContext.SaveChanges();
            //idlot ASP.NET Core, bypass it
            var result = Created();
            result.Value = post.PostId;
            return result;
        }
        else
            return Accepted(post.PostId);
    }

    [HttpGet]
    public IActionResult Status([FromQuery] long Id)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var post = dbContext.Posts.Find(Id);
        if (post is null || post.Deleted)
            return NotFound();
        else
            return Ok(post.Checked ? post.CheckSuccess ? 1 : 2 : 0);
    }

    [Authorize]
    [HttpPost]
    public IActionResult NewComment(NewPostCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest();
        var user = GetUserFromJwt();
        if (user is null)
            return Unauthorized();
        if (UserLockedOrBanned(user))
            return Forbid();
        using var dbContext = dbContextFactory.CreateDbContext();
        var post = dbContext.Posts.Find(request.PostId)
        if (post is null || post.Deleted)
            return NotFound();
        if (user.ThisHourPostCommentSend > PerHourPostCommentSendMax || user.ThisDayPostCommentSend > PerDayPostCommentSendMax)
            return StatusCode(StatusCodes.Status429TooManyRequests);
        user.ThisHourPostCommentSend++;
        user.ThisDayPostCommentSend++;
        var comment = new Types.DataBase.PostComment(request.PostId, user.Id, request.Content);
        dbContext.PostComments.Add(comment);
        dbContext.SaveChanges();
        var checkResult = baiduAi.TextCensorUserDefined(request.Content)["conclusion"].ToString();
        if (checkResult == "合规")
        {
            comment.Checked = true;
            comment.CheckSuccess = true;
            dbContext.SaveChanges();
            //idlot ASP.NET Core, bypass it
            var result = Created();
            result.Value = comment.CommentId;
            return result;
        }
        else
            return Accepted(comment.CommentId);
    }

    [HttpGet]
    public IActionResult CommentStatus([FromQuery] long Id)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var comment = dbContext.PostComments.Find(Id);
        if (comment is null || comment.Deleted)
            return NotFound();
        else
            return Ok(comment.Checked ? comment.CheckSuccess ? 1 : 2 : 0);
    }

    [HttpGet]
    public IActionResult Index([FromQuery] long Id)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var post = dbContext.Posts.Find(Id);
        if (post is null || post.Deleted)
            return NotFound();
        if (!post.CheckSuccess)
            return StatusCode(StatusCodes.Status451UnavailableForLegalReasons);
        var comments = (from comment in dbContext.PostComments
                        where comment.CheckSuccess && !comment.Deleted && comment.PostId == Id
                        orderby comment.SendTime descending
                        select new Shared.Results.PostComment()
                        {
                            Content = comment.Content,
                            SenderId = comment.SenderId,
                            SendTime = comment.SendTime,
                            SenderName = dbContext.Accounts.Find(comment.SenderId).Name
                        }
                        ).ToArray();
        var result = new GetPostIndexResult()
        {
            Comments = comments,
            Content = post.Content,
            SenderId = post.SenderId,
            SendTime = post.SendTime,
            Title = post.Title,
            SenderName = dbContext.Accounts.Find(post.SenderId).Name
        };
        return Ok(result);
    }

    [HttpGet]
    public IActionResult All()
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        return Ok((from post in dbContext.Posts
                   where post.Checked && !post.Deleted
                   orderby post.SendTime descending
                   select new GetAllPostPart()
                   {
                       Id = post.PostId,
                       CommentNuber = dbContext.PostComments.Where(comment => comment.PostId == post.PostId && !comment.Deleted).Count(),
                       SenderId = post.SenderId,
                       SenderName = dbContext.Accounts.Find(post.SenderId).Name,
                       SendTime = post.SendTime,
                       Title = post.Title
                   }).ToArray());
    }
}