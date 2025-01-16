namespace Back.Types.DataBase

open System
open System.ComponentModel.DataAnnotations

type TreehollowComment(treehollowId: int64, senderId: int64, content: string) =
    class
        [<Key>]
        member val CommentId = 0L with get

        member val TreehollowId = treehollowId with get
        member val SenderId = senderId with get
        member val Content = content with get
        member val SendTime = DateTime.Now with get
        member val Checked = false with get, set
        member val CheckSucess = false with get, set
    end
