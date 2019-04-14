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

type TodoList = {
    Id : Id
    Name: string
    Tasks: Task list
    }
