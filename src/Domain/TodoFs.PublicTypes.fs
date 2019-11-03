namespace TodoFs.Domain

open TodoFs.Domain.Common

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

type Label = Label of string

type Todo = {
    Id: Id
    Name: Name
    Label: Label
    Tasks: Task list
    }
