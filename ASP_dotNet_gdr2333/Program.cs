using FurTree.Services;
using FurTree.Types.DataBase;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContextFactory<Context>(opt =>
    opt.UseSqlite("Data Source=main.db"));
builder.Services.AddHostedService<DbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
