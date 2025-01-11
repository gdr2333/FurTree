using FurTree.Types.DataBase;
using FurTree.Types.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurTree.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ApiController(IDbContextFactory<Context> dbContextFactory) : ControllerBase
{
    [HttpPut]
    public ActionResult SendMessage([FromBody] string message)
    {
        using var context = dbContextFactory.CreateDbContext();
        context.Messages.Add(new() { Content = message });
        context.SaveChanges();
        return Created();
    }

    [HttpPut]
    public ActionResult SendComment([FromBody] SendCommentRequest request)
    {
        using var context = dbContextFactory.CreateDbContext();
        context.Comments.Add(new() { Context = request.Content, MessageId = request.MessageId });
        context.SaveChanges();
        return Created();
    }

    [HttpGet]
    public ActionResult<Message[]> GetMessages()
    {
        using var context = dbContextFactory.CreateDbContext();
        return Ok((from message in context.Messages orderby message.Id descending select message).ToArray());
    }

    [HttpGet]
    public ActionResult<Comment[]> GetComments([FromQuery] long messageId)
    {
        using var context = dbContextFactory.CreateDbContext();
        return Ok((from comment in context.Comments where comment.MessageId == messageId orderby comment.CommentId descending select comment).ToArray());
    }

    [HttpDelete]
    public ActionResult DeleteMessage([FromQuery] long messageId)
    {
        using var context = dbContextFactory.CreateDbContext();
        var message = context.Messages.Find(messageId);
        if (message is null)
            return NotFound();
        else
        {
            context.Messages.Remove(message);
            context.SaveChanges();
            return Accepted();
        }
    }

    [HttpDelete]
    public ActionResult DeleteComment([FromQuery] long commentId)
    {
        using var context = dbContextFactory.CreateDbContext();
        var comment = context.Comments.Find(commentId);
        if (comment is null)
            return NotFound();
        else
        {
            context.Comments.Remove(comment);
            context.SaveChanges();
            return Accepted();
        }
    }
}