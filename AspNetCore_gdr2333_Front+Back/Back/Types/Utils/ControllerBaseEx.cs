using System.IdentityModel.Tokens.Jwt;
using Back.Types.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Back.Types.Utils;

public class ControllerBaseEx(IDbContextFactory<MainDataBase> dbContextFactory) : ControllerBase
{
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
        if (capcha is null || capcha.Result != capchaInput)
            return BadRequest();
        return null;
    }

    public Account? GetUserFromJwt()
    {
        var authHeader = Request.Headers.Authorization;
        if (string.IsNullOrEmpty(authHeader))
            return null;
        using var dbContext = dbContextFactory.CreateDbContext();
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authHeader);
        var claimString = (from claim in jwtSecurityToken.Claims where claim.Type == "UserId" select claim)?.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(claimString))
            return null;
        return dbContext.Accounts.Find(long.Parse(claimString));
    }

    public static bool UserLockedOrBanned(Account account) =>
        account.Locked || account.BannedTo >= DateTime.Now;
}