namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type Capcha(uuid: string, result: string) =
    class
        [<Key>]
        member val Uuid = uuid with get

        member val Result = result with get
        member val DeleteAt = DateTime.Now.AddHours 1 with get
    end
