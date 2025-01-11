using Microsoft.EntityFrameworkCore;

namespace FurTree.Types.DataBase;

public class Context(ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Message> Messages { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=main.db");
        options.UseLoggerFactory(loggerFactory);
    }
}