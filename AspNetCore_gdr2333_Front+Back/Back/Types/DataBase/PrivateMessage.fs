namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type PrivateMessage(senderId: int64, receiverId: int64, content: string) =
    class
        [<Key>]
        member val PrivateMessageId = 0L with get

        member val SenderId = senderId with get
        member val ReceiverId = receiverId with get
        member val Content = content with get
        member val SendTime = DateTime.Now with get
        member val Checked = false with get, set
        member val CheckSucess = false with get, set
    end
