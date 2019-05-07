namespace TodoFs.Common

type Id = private Id of int64 with
    member this.Value =
        let (Id value) = this
        value

module Id =
    let create () =
        ticks() |> Id
    let from x =
        Id x
