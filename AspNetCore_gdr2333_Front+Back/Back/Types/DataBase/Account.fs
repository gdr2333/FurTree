namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type Account(name: string, email: string, passwordHash: string) =
    class
        [<Key>]
        member val Id = 0L with get

        member val Name = name with get, set
        member val Email = email with get, set
        member val PasswordHash = passwordHash with get, set
        member val EmailConfirmed = false with get, set
        member val Locked = false with get, set
        member val BannedTo = DateTime.MinValue with get, set
        member val LastGrobalMessageReadTime = DateTime.MinValue with get, set

        member val ThisHourPostSend = 0 with get, set
        member val ThisDayPostSend = 0 with get, set
        member val ThisHourPostCommentSend = 0 with get, set
        member val ThisDayPostCommentSend = 0 with get, set
        member val ThisHourTreehollowSend = 0 with get, set
        member val ThisDayTreehollowSend = 0 with get, set
        member val ThisHourTreehollowCommentSend = 0 with get, set
        member val ThisDayTreehollowCommentSend = 0 with get, set
        member val ThisDayUnresivedPrivateMessageSend = 0 with get, set
        member val LastConfirmEmailSendTime = DateTime.Now with get, set
    end
