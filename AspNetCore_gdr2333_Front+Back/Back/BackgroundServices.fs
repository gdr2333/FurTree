namespace Back

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Back.Types.DataBase
open System
open System.IO
open System.Threading
open System.Threading.Tasks

type BackgroundService(dataBase: DataBase IDbContextFactory) =
    class
        member val CheckEmailConfirmCodeOutdatedCancellationTokenSource = new CancellationTokenSource()
        member val CheckCapchaOutdatedCancellationTokenSource = new CancellationTokenSource()
        member val ResetHourCounterCancellationTokenSource = new CancellationTokenSource()
        member val ResetDayCounterCancellationTokenSource = new CancellationTokenSource()

        interface IHostedService with
            member this.StartAsync(cancellationToken: CancellationToken) : Task =
                task {
                    this.ConfirmDbCrated()
                    this.CheckCapchaOutdated this.CheckCapchaOutdatedCancellationTokenSource.Token |> Async.Start
                    this.CheckEmailConfirmCodeOutdated this.CheckEmailConfirmCodeOutdatedCancellationTokenSource.Token |> Async.Start
                    this.ResetDayCounter this.ResetDayCounterCancellationTokenSource.Token |> Async.Start
                    this.ResetHourCounter this.ResetHourCounterCancellationTokenSource.Token |> Async.Start
                }
                
            member this.StopAsync(cancellationToken: CancellationToken) : Task =
                task {
                    this.CheckCapchaOutdatedCancellationTokenSource.Cancel()
                    this.CheckEmailConfirmCodeOutdatedCancellationTokenSource.Cancel()
                    this.ResetDayCounterCancellationTokenSource.Cancel()
                    this.ResetHourCounterCancellationTokenSource.Cancel()
                    this.CheckCapchaOutdatedCancellationTokenSource.Dispose()
                    this.CheckEmailConfirmCodeOutdatedCancellationTokenSource.Dispose()
                    this.ResetDayCounterCancellationTokenSource.Dispose()
                    this.ResetHourCounterCancellationTokenSource.Dispose()
                }

        member this.CheckEmailConfirmCodeOutdated(cancellationToken: CancellationToken) =
            async {
                while not cancellationToken.IsCancellationRequested do
                    use mainDbContext = dataBase.CreateDbContext()

                    for confirmCode in
                        query {
                            for confirmCode in mainDbContext.EmailConfirmCodes do
                                where (confirmCode.DeleteAt > DateTime.Now)
                                select confirmCode
                        } do
                        mainDbContext.EmailConfirmCodes.Remove confirmCode |> ignore

                    mainDbContext.SaveChanges() |> ignore
                    do! Task.Delay(60000, cancellationToken) |> Async.AwaitTask
            }

        member this.CheckCapchaOutdated(cancellationToken: CancellationToken) =
            async {
                while not cancellationToken.IsCancellationRequested do
                    use mainDbContext = dataBase.CreateDbContext()

                    for capcha in
                        query {
                            for capcha in mainDbContext.Capchas do
                                where (capcha.DeleteAt > DateTime.Now)
                                select capcha
                        } do
                        mainDbContext.Capchas.Remove capcha |> ignore

                    mainDbContext.SaveChanges() |> ignore
                    do! Task.Delay(60000, cancellationToken) |> Async.AwaitTask
            }

        member this.ConfirmDbCrated() =
            if not (File.Exists "main.db") then
                use mainDbContext = dataBase.CreateDbContext()
                mainDbContext.Database.EnsureCreated() |> ignore

        member this.ResetHourCounter(cancellationToken: CancellationToken) =
            async {
                while not cancellationToken.IsCancellationRequested do
                    use mainDbContext = dataBase.CreateDbContext()

                    for account in mainDbContext.Accounts do
                        account.ThisHourPostCommentSend <- 0
                        account.ThisHourPostSend <- 0
                        account.ThisHourTreehollowCommentSend <- 0
                        account.ThisHourTreehollowSend <- 0

                    mainDbContext.SaveChanges() |> ignore
                    do! Task.Delay(TimeSpan(1, 0, 0), cancellationToken) |> Async.AwaitTask
            }

        member this.ResetDayCounter(cancellationToken: CancellationToken) =
            async {
                while not cancellationToken.IsCancellationRequested do
                    use mainDbContext = dataBase.CreateDbContext()

                    for account in mainDbContext.Accounts do
                        account.ThisDayPostCommentSend <- 0
                        account.ThisDayPostSend <- 0
                        account.ThisDayTreehollowCommentSend <- 0
                        account.ThisDayTreehollowSend <- 0
                        account.ThisDayUnresivedPrivateMessageSend <- 0

                    mainDbContext.SaveChanges() |> ignore
                    do! Task.Delay(TimeSpan(1, 0, 0, 0), cancellationToken) |> Async.AwaitTask
            }
    end
