namespace TodoFs

open TodoFs.Common

type Status =
    | Undone
    | Done

type Task = {
    Id: Id
    Title: string
    Status: Status
    }

type Name = Name of string with
    member this.Value =
        let (Name value) = this
        value

type Todo = {
    Id: Id
    Name: Name
    Tasks: Task list
    }
