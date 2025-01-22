using Back.Types.DataBase;
using Back.Types.Interface;
using Back.Types.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Request;

namespace Back.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController(IDbContextFactory<MainDataBase> dbContextFactory, IEmailSender emailSender, JwtHelper jwtHelper) : ControllerBaseEx(dbContextFactory)
{
    [HttpPut]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
            return vcres;
        using var dbContext = dbContextFactory.CreateDbContext();
        if (dbContext.Accounts.Any(account => account.Name == request.Name || account.Email == request.Email))
            return Conflict();
        var account = new Account(request.Name, request.Email, Convert.FromBase64String(request.PasswordHash));
        dbContext.Accounts.Add(account);
        dbContext.SaveChanges();
        return await SendConfirmEmailInternel(request.Email) ?? NoContent();
    }

    [HttpGet]
    public IActionResult Confirm([FromQuery] string requestId)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        if (string.IsNullOrEmpty(requestId))
            return BadRequest();
        var confirmTarget = dbContext.EmailConfirmCodes.Find(requestId);
        if (confirmTarget is null)
            return NotFound();
        dbContext.SaveChanges();
        var confirmUser = dbContext.Accounts.Find(confirmTarget.AccountId);
        if (confirmUser is null)
            return NotFound();
        if (confirmUser.EmailConfired)
            return Conflict();
        confirmUser.EmailConfired = true;
        dbContext.SaveChanges();
        return Ok("邮箱已确认。");
    }

    private async Task<IActionResult?> SendConfirmEmailInternel(string email)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        if (!dbContext.Accounts.Any(account => account.Email == email))
            return NotFound();
        var account = (from acc in dbContext.Accounts where acc.Email == email select acc).FirstOrDefault();
        if (account is null)
            return NotFound();
        if (account.EmailConfired)
            return Conflict();
        if (DateTime.Now - account.LastConfirmEmailSendTime <= new TimeSpan(1, 0, 0))
            return StatusCode(StatusCodes.Status429TooManyRequests);
        if (account.Locked)
            return Forbid();
        var confirmCode = new EmailConfirmCode(email, account.Id);
        dbContext.EmailConfirmCodes.Add(confirmCode);
        account.LastConfirmEmailSendTime = DateTime.Now;
        dbContext.SaveChanges();
        await emailSender.SendEmail(email, "账号确认", $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\" lang=\"zh-Hans\">\r\n<title>邮件确认</title>\r\n</head>\r\n<body>\r\n\r\n\t<a href=\"http://{Request.Host}/Account/Confirm?requestId={confirmCode.ConfirmCode}\">点击这里确认FurTree的账户创建确认邮件</a>\r\n\r\n</body>\r\n</html>");
        return null;
    }

    public async Task<IActionResult> ResendConfirm(ResendConfirmRequest request)
    {
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
            return vcres;
        using var dbContext = dbContextFactory.CreateDbContext();
        return await SendConfirmEmailInternel(request.Email) ?? Ok();
    }

    [HttpPost]
    public IActionResult Login(AccountLoginRequest request)
    {
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
            return vcres;
        using var dbContext = dbContextFactory.CreateDbContext();
        var user = (from account in dbContext.Accounts where account.Email == request.Name || account.Name == request.Name select account).FirstOrDefault();
        if (user is null)
            return NotFound();
        if (user.Locked)
            return Forbid();
        if (user.PasswordHash.SequenceEqual(Convert.FromBase64String(request.PasswordHash)))
            return Ok(jwtHelper.CreateToken(user.IsAdmin, user.Id));
        else
            return Unauthorized();
    }

    [Authorize]
    [HttpPut]
    public IActionResult Name(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return BadRequest();
        var user = GetUserFromJwt();
        if (user is null)
            return Unauthorized();
        if (user.Name == newName)
            return Conflict();
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.Name = newName;
        dbContext.SaveChanges();
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Email(string newEmail)
    {
        if (string.IsNullOrEmpty(newEmail))
            return BadRequest();
        var user = GetUserFromJwt();
        if (user is null)
            return Unauthorized();
        if (user.Email == newEmail)
            return Conflict();
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.Email = newEmail;
        targetUser.EmailConfired = false;
        dbContext.SaveChanges();
        var req = await SendConfirmEmailInternel(newEmail);
        if (req is not null)
            return req;
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public IActionResult Password(string newPaswordHash)
    {
        if (string.IsNullOrEmpty(newPaswordHash))
            return BadRequest();
        var user = GetUserFromJwt();
        if (user is null)
            return Unauthorized();
        var newPasswordHashArray = Convert.FromBase64String(newPaswordHash);
        if (user.PasswordHash.SequenceEqual(newPasswordHashArray))
            return Conflict();
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.PasswordHash = newPasswordHashArray;
        dbContext.SaveChanges();
        return NoContent();
    }
}