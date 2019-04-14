﻿module internal TodoFs.Implementation

open System
open TodoFs
open TodoFs.Common

// Task

let createTask title =
    {Id = newId (); Title = title; Status = Undone}

let renameTask task title =
    {task with Title = title}

let doTask task =
    {task with Status = Done}


// TodoList

let createTodo name =
    {Id = newId (); Name = name; Tasks = []}

let addTask todo task  =
    {todo with Tasks = task :: todo.Tasks}

let markDone todo (task: Task)  =
    let markDoneOrPassthru (t: Task) =
        if t.Id = task.Id
        then doTask t
        else t
    let tasks = todo.Tasks |> List.map markDoneOrPassthru
    {todo with Tasks = tasks}

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