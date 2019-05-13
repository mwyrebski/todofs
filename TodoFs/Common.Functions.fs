namespace TodoFs.Common

open System

[<AutoOpen>]
module Utils =
    let inline ticks () =
        DateTime.UtcNow.Ticks
    let inline tee f x = f x; x
