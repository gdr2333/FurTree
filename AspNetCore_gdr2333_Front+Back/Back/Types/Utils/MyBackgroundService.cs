using Back.Types.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Back.Types.Utils
{
    public class MyBackgroundService(IDbContextFactory<MainDataBase> dbContextFactory) : IHostedService
    {
        private CancellationTokenSource _checkEmailConfirmCodeOutdatedCancellationTokenSource = new();
        private CancellationTokenSource _checkCapchaOutdatedCancellationTokenSource = new();
        private CancellationTokenSource _resetHourCounterCancellationTokenSource = new();
        private CancellationTokenSource _resetDayCounterCancellationTokenSource = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ConfirmDbCrated();
            CheckCapchaOutdated(_checkCapchaOutdatedCancellationTokenSource.Token);
            CheckEmailConfirmCodeOutdated(_checkEmailConfirmCodeOutdatedCancellationTokenSource.Token);
            ResetDayCounter(_resetDayCounterCancellationTokenSource.Token);
            ResetHourCounter(_resetHourCounterCancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _checkCapchaOutdatedCancellationTokenSource.Cancel();
            _checkEmailConfirmCodeOutdatedCancellationTokenSource.Cancel();
            _resetDayCounterCancellationTokenSource.Cancel();
            _resetHourCounterCancellationTokenSource.Cancel();

            _checkCapchaOutdatedCancellationTokenSource.Dispose();
            _checkEmailConfirmCodeOutdatedCancellationTokenSource.Dispose();
            _resetDayCounterCancellationTokenSource.Dispose();
            _resetHourCounterCancellationTokenSource.Dispose();

            return Task.CompletedTask;
        }

        private async Task CheckEmailConfirmCodeOutdated(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var mainDbContext = dbContextFactory.CreateDbContext())
                {
                    if (mainDbContext.EmailConfirmCodes != null)
                    {
                        foreach (var confirmCode in mainDbContext.EmailConfirmCodes)
                        {
                            if (confirmCode.DeleteAt < DateTime.Now)
                            {
                                mainDbContext.EmailConfirmCodes.Remove(confirmCode);
                            }
                        }
                    }

                    await mainDbContext.SaveChangesAsync(cancellationToken);
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private async Task CheckCapchaOutdated(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var mainDbContext = dbContextFactory.CreateDbContext())
                {
                    if (mainDbContext.Capchas != null)
                    {
                        foreach (var capcha in mainDbContext.Capchas)
                        {
                            if (capcha.DeleteAt < DateTime.Now)
                            {
                                mainDbContext.Capchas.Remove(capcha);
                            }
                        }
                    }

                    await mainDbContext.SaveChangesAsync(cancellationToken);
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private void ConfirmDbCrated()
        {
            if (!File.Exists("main.db"))
            {
                using (var mainDbContext = dbContextFactory.CreateDbContext())
                {
                    mainDbContext.Database.EnsureCreated();
                    mainDbContext.SaveChanges();
                }
            }
        }

        private async Task ResetHourCounter(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var mainDbContext = dbContextFactory.CreateDbContext())
                {
                    if (mainDbContext.Accounts != null)
                    {
                        foreach (var account in mainDbContext.Accounts)
                        {
                            account.ThisHourPostCommentSend = 0;
                            account.ThisHourPostSend = 0;
                            account.ThisHourTreehollowCommentSend = 0;
                            account.ThisHourTreehollowSend = 0;
                        }
                    }

                    await mainDbContext.SaveChangesAsync(cancellationToken);
                }

                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }
        }

        private async Task ResetDayCounter(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var mainDbContext = dbContextFactory.CreateDbContext())
                {
                    if (mainDbContext.Accounts != null)
                    {
                        foreach (var account in mainDbContext.Accounts)
                        {
                            account.ThisDayPostCommentSend = 0;
                            account.ThisDayPostSend = 0;
                            account.ThisDayTreehollowCommentSend = 0;
                            account.ThisDayTreehollowSend = 0;
                            account.ThisDayUnresivedPrivateMessageSend = 0;
                        }
                    }

                    await mainDbContext.SaveChangesAsync(cancellationToken);
                }

                await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
            }
        }
    }
}
