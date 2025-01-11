using Microsoft.EntityFrameworkCore;
using FurTree.Types.DataBase;

namespace FurTree.Services;
public class DbService(IDbContextFactory<Context> dbContextFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if(!File.Exists("main.db"))
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}