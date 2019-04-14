namespace TodoFs.Common

open System

[<AutoOpen>]
module Utils =
    let inline newId () =
        Id DateTime.UtcNow.Ticks
