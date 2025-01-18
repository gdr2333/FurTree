using Back.Types.DataBase;
using Back.Types.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<MainDataBase>(opt => opt.UseSqlite());
builder.Services.AddHostedService<MyBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
