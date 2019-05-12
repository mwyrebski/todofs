
// common types

type Id = Id of int64


// domain types

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


// common functions

open System

[<AutoOpen>]
module Utils =
    let inline newId() =
        Id DateTime.UtcNow.Ticks


// domain functions

module Task =

    let create title =
        { Id = newId(); Title = title; Status = Undone }

    let rename task title =
        { task with Title = title }

    let markDone task =
        { task with Status = Done }


module Todo =

    let create name =
        { Id = newId(); Name = name; Tasks = [] }

    let addTask todo task =
        { todo with Tasks = task :: todo.Tasks }

    let markDone todo (task: Task) =
        let markDoneOrPassthru (t: Task) =
            if t.Id = task.Id
            then Task.markDone t
            else t
        let tasks = todo.Tasks |> List.map markDoneOrPassthru
        { todo with Tasks = tasks }

    let display todo =
        let doneChar task =
            match task.Status with
            | Done -> 'x'
            | Undone -> ' '
        let tasks =
            todo.Tasks
            |> Seq.map (fun t -> sprintf "[%c] %s" (doneChar t) t.Title)
        seq {
            let header = sprintf "..:: %s ::.." todo.Name
            yield String.Empty
            yield header
            yield String.replicate (String.length header) "-"
            yield! tasks
            yield String.Empty
        }
        |> String.concat Environment.NewLine
        |> printfn "%s"



// tests

let myEmptyList = Todo.create "List of things to do"

let myTasks = [
    Task.markDone (Task.create "Possibility of creating task lists")
    Task.create "Handle Done task status" |> Task.markDone
    Task.create "Add labels"
    Task.create "Add helper for displaying task list" |> Task.markDone
    Task.create "Add comments"
    Task.create "Add reminders"
 ]

// add all myTasks to myEmptyList
let myList = List.fold (fun list task -> Todo.addTask list task) myEmptyList myTasks

Todo.display myList;


// create new task, rename it and add to myList

let extraTask = Task.create "Add ..."
let extraTask' = Task.rename extraTask "Add possiblity of task renaming"
let myListWithExtraTask = Todo.addTask myList extraTask'
let myCompleteList = Todo.markDone myListWithExtraTask extraTask'

Todo.display myCompleteList
