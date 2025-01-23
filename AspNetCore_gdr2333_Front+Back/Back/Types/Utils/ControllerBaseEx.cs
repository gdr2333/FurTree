using System.IdentityModel.Tokens.Jwt;
using Back.Types.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Back.Types.Utils;

public class ControllerBaseEx(IDbContextFactory<MainDataBase> dbContextFactory, ILoggerFactory loggerFactory) : ControllerBase
{
    private ILogger<ControllerBaseEx> _cbex_logger = loggerFactory.CreateLogger<ControllerBaseEx>();
    public IActionResult? ValidCapcha(string capchaId, string capchaInput)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var capcha = dbContext.Capchas.Find(capchaId);
        //不管发生什么，这个验证码都得吊销了
        if (capcha is not null)
        {
            dbContext.Capchas.Remove(capcha);
            dbContext.SaveChanges();
        }
        _cbex_logger.LogInformation($"收到验证码确认请求：Id：{capchaId}，预期：{capcha?.Result}，答案：{capchaInput}");
        if (capcha is null || capcha.Result != capchaInput)
        {
            _cbex_logger.LogWarning("验证码验证请求：失败");
            return BadRequest();
        }
        _cbex_logger.LogInformation("验证码验证请求：成功");
        return null;
    }

    public Account? GetUserFromJwt()
    {
        var authHeader = Request.Headers.Authorization;
        _cbex_logger.LogInformation($"收到用户身份查询请求：Authorization头：{authHeader}");
        if (string.IsNullOrEmpty(authHeader))
        {
            _cbex_logger.LogWarning($"非法用户身份查询请求：未提交Authorization头");
            return null;
        }
        using var dbContext = dbContextFactory.CreateDbContext();
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authHeader);
        var claimString = (from claim in jwtSecurityToken.Claims where claim.Type == "UserId" select claim)?.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(claimString))
        {
            _cbex_logger.LogWarning($"非法用户身份查询请求：没有UserId的Claim，怀疑是伪造的JWT");
            return null;
        }
        return dbContext.Accounts.Find(long.Parse(claimString));
    }

    public static bool UserLockedOrBanned(Account account) =>
        account.Locked || account.BannedTo >= DateTime.Now;
}