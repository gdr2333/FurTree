using System.Text;
using Back.Types.DataBase;
using Back.Types.Interface;
using Back.Types.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<MainDataBase>(opt => opt.UseSqlite());
builder.Services.AddSingleton<IEmailSender>((_) => new EmailSender(builder.Configuration));
builder.Services.AddHostedService<MyBackgroundService>();
builder.Services.AddSingleton(new JwtHelper(builder.Configuration));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        RequireExpirationTime = true,
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
