namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type Treehollow(senderId: int64, content: string, isPublic: bool) =
    class
        [<Key>]
        member val TreehollowId = 0L with get

        member val SenderId = senderId with get
        member val Content = content with get
        member val SendTime = DateTime.Now with get
        member val Checked = false with get, set
        member val CheckSucess = false with get, set
        member val IsPublic = isPublic with get, set
    end
