namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type PostComment(senderId:int64,content:string,sendTime:DateTime) = 
    class
        [<Key>]
        member val PostId = 0L with get
        member val SenderId = senderId with get
        member val Content = content with get
        member val SendTime = sendTime with get
        member val Checked = false with get, set
        member val CheckSucess = false with get, set
    end