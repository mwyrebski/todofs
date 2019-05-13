module TodoFs.Implementation

open System
open TodoFs
open TodoFs.Common

// Tasks

let createTask title =
    {Id = Id.create(); Title = title; Status = Undone}

let renameTask task title =
    {task with Title = title}

let doTask task =
    {task with Status = Done}


// Todos

let createTodo name =
    {Id = Id.create(); Name = name; Tasks = []}
    
let renameTodo todo name =
    {todo with Name = name}

let addTask todo task =
    {todo with Tasks = task :: todo.Tasks}

let addTasks todo tasks =
    {todo with Tasks = tasks @ todo.Tasks}

let markDone todo (task: Task)  =
    let markDoneOrPassthru (t: Task) =
        if t.Id = task.Id
        then doTask t
        else t
    let tasks = todo.Tasks |> List.map markDoneOrPassthru
    {todo with Tasks = tasks}

let toString todo =
    let doneChar task =
        match task.Status with
        | Done -> 'x'
        | Undone -> ' '
    let tasks =
        todo.Tasks
        |> Seq.map (fun t -> sprintf "[%c] %s" (doneChar t) t.Title)
    seq {
        let header = sprintf "..:: %s ::.." todo.Name.Value
        yield String.Empty
        yield header
        yield String.replicate (String.length header) "-" 
        yield! tasks
        yield String.Empty
    }
    |> String.concat Environment.NewLine
