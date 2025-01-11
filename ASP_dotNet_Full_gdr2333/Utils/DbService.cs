using FurTreeFull.Data.DataBase;
using Microsoft.EntityFrameworkCore;

namespace FurTreeFull.Utils;

public class DbService(IDbContextFactory<MainDb> dbContextFactory) : IHostedService
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