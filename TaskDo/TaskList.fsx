
// common types

type Id = Id of int64


// domain types

type TaskStatus =
    | Undone
    | Done

type TaskData = {
    Id: Id
    Title: string
    Status: TaskStatus
}

type TaskList = {
    Id : Id
    Name: string
    Tasks: TaskData list
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


module TaskList =
    
    let create name =
        {Id = newId (); Name = name; Tasks = []}

    let addTask tasklist task  =
        {tasklist with Tasks = task :: tasklist.Tasks}

    let markDone tasklist (task: TaskData)  =
        let markDoneOrPassthru (t: TaskData) =
            if t.Id = task.Id
            then Task.markDone t
            else t
        let tasks = tasklist.Tasks |> List.map markDoneOrPassthru
        {tasklist with Tasks = tasks}

    let display tasklist =
        let doneChar task =
            match task.Status with
            | Done -> 'x'
            | Undone -> ' '
        let tasks =
            tasklist.Tasks
            |> Seq.map (fun t -> sprintf "[%c] %s" (doneChar t) t.Title)
        seq {
            let header = sprintf "..:: %s ::.." tasklist.Name
            yield String.Empty
            yield header
            yield String.replicate (String.length header) "-" 
            yield! tasks
        }
        |> String.concat Environment.NewLine
        |> printfn "%s"
        // |> List.iter printfn



// tests


let myEmptyList = TaskList.create "List of things to do"

let myTasks = [
    Task.create "Possibility of creating task lists" |> Task.markDone
    Task.create "Handle Done task status" |> Task.markDone
    Task.create "Add labels"
    Task.create "Add helper for displaying task list" |> Task.markDone
    Task.create "Add comments"
    Task.create "Add reminders"
]

// let myList = myTasks |> List.map addToMyList ???
let myList = List.fold (fun list task -> TaskList.addTask list task) myEmptyList myTasks

TaskList.display myList;;

// with one more task after renaming it

let extraTask = Task.create "Add ..."
let extraTask' = Task.rename extraTask "Add possiblity of task renaming"
let myListWithExtraTask = TaskList.addTask myList extraTask'
let myCompleteList = TaskList.markDone myListWithExtraTask extraTask'

TaskList.display myCompleteList;;
