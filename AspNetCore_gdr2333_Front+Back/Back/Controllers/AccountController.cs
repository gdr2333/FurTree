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
public class AccountController(IDbContextFactory<MainDataBase> dbContextFactory, IEmailSender emailSender, JwtHelper jwtHelper, ILoggerFactory loggerFactory) : ControllerBaseEx(dbContextFactory, loggerFactory)
{
    private ILogger<AccountController> _logger = loggerFactory.CreateLogger<AccountController>();
    [HttpPut]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        _logger.LogInformation($"收到用户创建请求：用户名：{request.Name}，邮箱：{request.Email}");
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
        {
            _logger.LogWarning($"用户创建请求失败：验证码错误");
            return vcres;
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        if (dbContext.Accounts.Any(account => account.Name == request.Name || account.Email == request.Email))
        {
            _logger.LogWarning($"用户创建请求失败：用户名或邮箱冲突");
            return Conflict();
        }
        var account = new Account(request.Name, request.Email, Convert.FromBase64String(request.PasswordHash));
        dbContext.Accounts.Add(account);
        dbContext.SaveChanges();
        var sceir = await SendConfirmEmailInternel(request.Email);
        if(sceir is not null)
        {
            _logger.LogWarning($"用户创建请求失败：确认邮件发送过程中出现错误");
            return sceir;
        }
        _logger.LogInformation("用户创建请求成功");
        return NoContent();
    }

    [HttpGet]
    public IActionResult Confirm([FromQuery] string requestId)
    {
        _logger.LogInformation($"开始邮箱确认流程，请求ID：{requestId}");
        using var dbContext = dbContextFactory.CreateDbContext();
        if (string.IsNullOrEmpty(requestId))
        {
            _logger.LogWarning("邮箱确认失败：请求ID为空");
            return BadRequest();
        }
        var confirmTarget = dbContext.EmailConfirmCodes.Find(requestId);
        if (confirmTarget is null)
        {
            _logger.LogWarning("邮箱确认失败：未找到对应的确认码");
            return NotFound();
        }
        dbContext.SaveChanges();
        var confirmUser = dbContext.Accounts.Find(confirmTarget.AccountId);
        if (confirmUser is null)
        {
            _logger.LogWarning("邮箱确认失败：未找到对应的用户");
            return NotFound();
        }
        if (confirmUser.EmailConfired)
        {
            _logger.LogWarning("邮箱确认失败：邮箱已被确认");
            return Conflict();
        }
        confirmUser.EmailConfired = true;
        dbContext.SaveChanges();
        _logger.LogInformation("邮箱确认成功");
        return Ok("邮箱已确认。");
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

    public async Task<IActionResult> ResendConfirm(ResendConfirmRequest request)
    {
        _logger.LogInformation($"开始重新发送确认邮件，邮箱：{request.Email}");
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
        {
            _logger.LogWarning("重新发送确认邮件失败：验证码错误");
            return vcres;
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var result = await SendConfirmEmailInternel(request.Email);
        if (result is not null)
        {
            _logger.LogWarning("重新发送确认邮件失败");
            return result;
        }
        _logger.LogInformation("重新发送确认邮件成功");
        return Ok();
    }

    [HttpPost]
    public IActionResult Login(AccountLoginRequest request)
    {
        _logger.LogInformation($"开始登录流程，用户名/邮箱：{request.Name}");
        var vcres = ValidCapcha(request.CapchaId, request.CapchaResult);
        if (vcres is not null)
        {
            _logger.LogWarning("登录失败：验证码错误");
            return vcres;
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var user = (from account in dbContext.Accounts where account.Email == request.Name || account.Name == request.Name select account).FirstOrDefault();
        if (user is null)
        {
            _logger.LogWarning("登录失败：未找到用户");
            return NotFound();
        }
        if (user.Locked)
        {
            _logger.LogWarning("登录失败：账户被锁定");
            return Forbid();
        }
        if (user.PasswordHash.SequenceEqual(Convert.FromBase64String(request.PasswordHash)))
        {
            _logger.LogInformation("登录成功");
            return Ok(jwtHelper.CreateToken(user.IsAdmin, user.Id));
        }
        else
        {
            _logger.LogWarning("登录失败：密码错误");
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpPut]
    public IActionResult Name([FromBody] string newName)
    {
        _logger.LogInformation($"开始修改用户名流程，新用户名：{newName}");
        if (string.IsNullOrEmpty(newName))
        {
            _logger.LogWarning("修改用户名失败：新用户名为空");
            return BadRequest();
        }
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("修改用户名失败：用户未授权");
            return Unauthorized();
        }
        if (user.Name == newName)
        {
            _logger.LogWarning("修改用户名失败：新用户名与原用户名相同");
            return Conflict();
        }
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.Name = newName;
        dbContext.SaveChanges();
        _logger.LogInformation("修改用户名成功");
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Email([FromBody] string newEmail)
    {
        _logger.LogInformation($"开始修改邮箱流程，新邮箱：{newEmail}");
        if (string.IsNullOrEmpty(newEmail))
        {
            _logger.LogWarning("修改邮箱失败：新邮箱为空");
            return BadRequest();
        }
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("修改邮箱失败：用户未授权");
            return Unauthorized();
        }
        if (user.Email == newEmail)
        {
            _logger.LogWarning("修改邮箱失败：新邮箱与原邮箱相同");
            return Conflict();
        }
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.Email = newEmail;
        targetUser.EmailConfired = false;
        var req = await SendConfirmEmailInternel(newEmail);
        if (req is not null)
        {
            _logger.LogWarning("修改邮箱失败：发送确认邮件过程中出现错误");
            return req;
        }
        dbContext.SaveChanges();
        _logger.LogInformation("修改邮箱成功");
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public IActionResult Password([FromBody] string newPaswordHash)
    {
        _logger.LogInformation("开始修改密码流程");
        if (string.IsNullOrEmpty(newPaswordHash))
        {
            _logger.LogWarning("修改密码失败：新密码哈希为空");
            return BadRequest();
        }
        var user = GetUserFromJwt();
        if (user is null)
        {
            _logger.LogWarning("修改密码失败：用户未授权");
            return Unauthorized();
        }
        var newPasswordHashArray = Convert.FromBase64String(newPaswordHash);
        if (user.PasswordHash.SequenceEqual(newPasswordHashArray))
        {
            _logger.LogWarning("修改密码失败：新密码与原密码相同");
            return Conflict();
        }
        var dbContext = dbContextFactory.CreateDbContext();
        var targetUser = dbContext.Accounts.Find(user.Id);
        targetUser.PasswordHash = newPasswordHashArray;
        dbContext.SaveChanges();
        _logger.LogInformation("修改密码成功");
        return NoContent();
    }
}