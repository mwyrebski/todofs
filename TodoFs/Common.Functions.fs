namespace TodoFs.Common

open System

[<AutoOpen>]
module Utils =
    let inline ticks () =
        DateTime.UtcNow.Ticks
