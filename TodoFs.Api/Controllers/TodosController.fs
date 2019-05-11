namespace TodoFs.Api.Controllers

open TodoFs.Api.Data
open Microsoft.AspNetCore.Mvc
open TodoFs
open TodoFs.Common
open TodoFs.Implementation

type TodoDto = {
    Id: int64
    Name: string
    TasksCount: int
    }
and TaskDto = {
    Id: int64
    Title: string
    Status: StatusDto
    }
and StatusDto =
    | Undone = 0
    | Done = 1


type TodoIdParam = { todoId: int64 }


[<Route("api/[controller]")>]
[<ApiController>]
type TodosController(repo: TodosRepository) =
    inherit ControllerBase()

    let toTodoDto (x: TodoList) =
        { Id = x.Id.Value; Name = x.Name; TasksCount = List.length x.Tasks }

    let toTaskDto (x: Task) =
        let status =
            match x.Status with
            | Undone -> StatusDto.Undone
            | Done -> StatusDto.Done
        { Id = x.Id.Value; Title = x.Title; Status = status }

    let toStatus s =
        match s with
        | StatusDto.Undone -> Status.Undone
        | StatusDto.Done -> Status.Done
        | _ -> failwith "Unknown status"
        
    let tryFindTask id = List.tryFind (fun (x: Task) -> x.Id = id)
    
    member private this.ok x = this.Ok x :> IActionResult
    member private this.notFound () = this.NotFound() :> IActionResult
        
    [<HttpGet>]
    member this.GetTodoLists() =
        let todos = repo.All() |> List.map toTodoDto
        ActionResult<TodoDto list>(todos)

    [<HttpGet("{todoId}")>]
    member this.GetTodoList(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> Option.fold (fun _ x -> x |> toTodoDto |> this.ok) (this.notFound())

    [<HttpGet("{todoId}/tasks")>]
    member this.GetTasks(todoId: int64) =
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo -> todo.Tasks |> List.map toTaskDto |> this.Ok :> IActionResult
        | None -> this.NotFound() :> IActionResult

    [<HttpGet("{todoId}/tasks/{taskId}")>]
    member this.GetTask(todoId: int64, taskId: int64) =
        let taskId = Id.from taskId
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            let taskOpt = tryFindTask taskId todo.Tasks
            match taskOpt with
            | Some task -> toTaskDto task |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPost>]
    member this.AddTodoList( [<FromBody>] name: string) =
        let todo = createTodo name
        repo.Upsert todo
        this.CreatedAtAction("GetTodoList", { todoId = todo.Id.Value }, todo |> toTodoDto)

    [<HttpPut("{todoId}")>]
    member this.RenameTodoList(todoId: int64,  [<FromBody>] name: string) =
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            renameTodo todo name
            |> repo.Upsert
            |> this.Ok :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPost("{todoId}/tasks")>]
    member this.AddTask(todoId: int64,  [<FromBody>] title: string) =
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            createTask title
            |> addTask todo
            |> repo.Upsert
            this.NoContent() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPatch("{todoId}/tasks/{taskId}")>]
    member this.RenameTask(todoId: int64, taskId: int64,  [<FromBody>] title: string) =
        let taskId = Id.from taskId
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            let taskOpt = tryFindTask taskId todo.Tasks
            match taskOpt with
            | Some task ->
                let renameOrPassthru x =
                    if x = task
                    then renameTask x title
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map renameOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member this.ChangeStatus(todoId: int64, taskId: int64,  [<FromQuery>] status: StatusDto) =
        let taskId = Id.from taskId
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            let taskOpt = tryFindTask taskId todo.Tasks
            match taskOpt with
            | Some task ->
                let replaceOrPassthru x =
                    if x = task
                    then { x with Status = status |> toStatus }
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map replaceOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpDelete("{todoId}")>]
    member this.DeleteTodoList(todoId: int64) =
        let removed = todoId |> Id.from |> repo.Remove
        if removed then
            this.Ok() :> IActionResult
        else
            this.NotFound() :> IActionResult

    [<HttpDelete("{todoId}/tasks/{taskId}")>]
    member this.DeleteTask(todoId: int64, taskId: int64) =
        let taskId = Id.from taskId
        let todoOpt = todoId |> Id.from |> repo.TryGet
        match todoOpt with
        | Some todo ->
            let taskOpt = tryFindTask taskId todo.Tasks
            match taskOpt with
            | Some task ->
                let newTodo = { todo with Tasks = todo.Tasks |> List.except [ task ] }
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult
