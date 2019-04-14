
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

type TodoList = {
    Id : Id
    Name: string
    Tasks: Task list
    }


// common functions

open System

[<AutoOpen>]
module Utils =
    let inline newId () =
        Id DateTime.UtcNow.Ticks


// domain functions

module Task =

    let create title =
        {Id = newId (); Title = title; Status = Undone}

    let rename task title =
        {task with Title = title}

    let markDone task =
        {task with Status = Done}


module TodoList =
    
    let create name =
        {Id = newId (); Name = name; Tasks = []}

    let addTask todolist task  =
        {todolist with Tasks = task :: todolist.Tasks}

    let markDone todolist (task: Task)  =
        let markDoneOrPassthru (t: Task) =
            if t.Id = task.Id
            then Task.markDone t
            else t
        let tasks = todolist.Tasks |> List.map markDoneOrPassthru
        {todolist with Tasks = tasks}

    let display todolist =
        let doneChar task =
            match task.Status with
            | Done -> 'x'
            | Undone -> ' '
        let tasks =
            todolist.Tasks
            |> Seq.map (fun t -> sprintf "[%c] %s" (doneChar t) t.Title)
        seq {
            let header = sprintf "..:: %s ::.." todolist.Name
            yield String.Empty
            yield header
            yield String.replicate (String.length header) "-" 
            yield! tasks
            yield String.Empty
        }
        |> String.concat Environment.NewLine
        |> printfn "%s"



// tests

let myEmptyList = TodoList.create "List of things to do"

let myTasks = [
    Task.markDone (Task.create "Possibility of creating task lists")
    Task.create "Handle Done task status" |> Task.markDone
    Task.create "Add labels"
    Task.create "Add helper for displaying task list" |> Task.markDone
    Task.create "Add comments"
    Task.create "Add reminders"
]

// add all myTasks to myEmptyList
let myList = List.fold (fun list task -> TodoList.addTask list task) myEmptyList myTasks

TodoList.display myList;;


// create new task, rename it and add to myList

let extraTask = Task.create "Add ..."
let extraTask' = Task.rename extraTask "Add possiblity of task renaming"
let myListWithExtraTask = TodoList.addTask myList extraTask'
let myCompleteList = TodoList.markDone myListWithExtraTask extraTask'

TodoList.display myCompleteList;;
