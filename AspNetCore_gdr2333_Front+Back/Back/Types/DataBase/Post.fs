namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type Post(senderId: int64, title: string, content: string) =
    class
        [<Key>]
        member val PostId = 0L with get

        member val SenderId = senderId with get
        member val Title = title with get
        member val Content = content with get
        member val SendTime = DateTime.Now with get
        member val Checked = false with get, set
        member val CheckSucess = false with get, set
    end
