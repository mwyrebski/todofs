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

type Todo = {
    Id: Id
    Name: string
    Tasks: Task list
    }
