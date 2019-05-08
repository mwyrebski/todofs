namespace TodoFs.Common

type Id = private Id of int64 with
    member this.Value =
        let (Id value) = this
        value

module Id =
    let mutable private _next = 0L
    let create () =
        let id = Id _next
        _next <- _next + 1L
        id
    let from x =
        Id x
