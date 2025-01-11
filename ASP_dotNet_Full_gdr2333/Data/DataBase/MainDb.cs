using Microsoft.EntityFrameworkCore;

namespace FurTreeFull.Data.DataBase;

public class MainDb(ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<Comment> Comments { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=main.db");
        options.UseLoggerFactory(loggerFactory);
    }
}