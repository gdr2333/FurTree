namespace Back.Types.DataBase

open Microsoft.EntityFrameworkCore

type DataBase(loggerFactory) =
    class
        inherit DbContext()
        member val Accounts : Account DbSet
        member val EmailConfirmCodes : EmailConfirmCode DbSet
        member val Capchas : Capcha DbSet
        member val Posts : Post DbSet
        member val PostComments : PostComment DbSet
        member val Treehollows : Treehollow DbSet
        member val TreehollowComments : TreehollowComment DbSet
        member val GlobalMessages : GlobalMessage DbSet
        member val PrivateMessages : PrivateMessage DbSet
        override this.OnConfiguring(options) =
            options.UseSqlite "Data Source=mainDb.db" |> ignore
            options.UseLoggerFactory loggerFactory |> ignore
    end