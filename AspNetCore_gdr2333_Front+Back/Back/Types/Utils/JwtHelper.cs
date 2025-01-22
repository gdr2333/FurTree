using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Back.Types.Utils;

public class JwtHelper
{
    private readonly IConfiguration _configuration;

    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(bool IsAdmin, long UserId)
    {
        // 1. 定义需要使用到的Claims
        Claim[] claims;
        if (IsAdmin)
        {
            claims =
            [
                new Claim(ClaimTypes.Name, "u_admin"), //HttpContext.User.Identity.Name
                new Claim(ClaimTypes.Role, "r_admin"), //HttpContext.User.IsInRole("r_admin")
                new Claim(JwtRegisteredClaimNames.Jti, "admin"),
                new Claim("UserId", UserId.ToString()),
                new Claim("Name", "管理员")
            ];
        }
        else
        {
            claims =
            [
                new Claim(ClaimTypes.Name, "u_user"), //HttpContext.User.Identity.Name
                new Claim(ClaimTypes.Role, "r_user"), //HttpContext.User.IsInRole("r_admin")
                new Claim(JwtRegisteredClaimNames.Jti, "user"),
                new Claim("UserId", UserId.ToString()),
                new Claim("Name", "用户")
            ];
        }

        // 2. 从 appsettings.json 中读取SecretKey
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

        // 3. 选择加密算法
        var algorithm = SecurityAlgorithms.HmacSha256;

        // 4. 生成Credentials
        var signingCredentials = new SigningCredentials(secretKey, algorithm);

        // 5. 根据以上，生成token
        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],     //Issuer
            _configuration["Jwt:Audience"],   //Audience
            claims,                          //Claims,
            DateTime.Now,                    //notBefore
            DateTime.Now.AddDays(7),    //expires
            signingCredentials               //Credentials
        );

        // 6. 将token变为string
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return token;
    }
}