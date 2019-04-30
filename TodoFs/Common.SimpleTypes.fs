namespace TodoFs.Common

type Id = private Id of int64

module Id =
    let create () =
        ticks() |> Id
