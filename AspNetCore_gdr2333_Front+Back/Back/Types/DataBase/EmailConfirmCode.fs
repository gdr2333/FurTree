namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type EmailConfirmCode(toEmail: string) =
    class
        let buffer: byte array = Array.zeroCreate 15
        do Random.Shared.NextBytes buffer

        [<Key>]
        member val ConfirmCode = Convert.ToBase64String buffer with get

        member val ToEmail = toEmail with get
        member val DeleteAt = DateTime.Now.AddDays 1
    end
