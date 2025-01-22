using Microsoft.EntityFrameworkCore;

namespace Back.Types.DataBase;

public class MainDataBase(ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<EmailConfirmCode> EmailConfirmCodes { get; set; }
    public DbSet<Capcha> Capchas { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<Treehollow> Treehollows { get; set; }
    public DbSet<TreehollowComment> TreehollowComments { get; set; }
    public DbSet<GlobalMessage> GlobalMessages { get; set; }
    public DbSet<PrivateMessage> PrivateMessages { get; set; }

    public ILoggerFactory LoggerFactory { get; } = loggerFactory;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=mainDb.db");
        options.UseLoggerFactory(LoggerFactory);
    }
}
