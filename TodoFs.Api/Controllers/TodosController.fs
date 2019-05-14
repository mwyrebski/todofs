namespace TodoFs.Api.Controllers

open System
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
type TodosController(repo: TodosRepository) as self =
    inherit ControllerBase()

    let toTodoDto (x: Todo) =
        { Id = x.Id.Value; Name = x.Name.Value; TasksCount = List.length x.Tasks }

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

    let createdAt actionName routeValues value =
        self.CreatedAtAction(actionName, routeValues, value) :> IActionResult
    let noContent() = self.NoContent() :> IActionResult
    let notFound() = self.NotFound() :> IActionResult
    let okOrNotFound f o =
        match o with
        | Some x -> f x |> self.Ok :> IActionResult
        | None -> self.NotFound() :> IActionResult
    let badRequest (e: obj) = e |> self.BadRequest :> IActionResult
    let ok x = x |> self.Ok :> IActionResult
    let toResponse okResp errResp result =
        match result with
        | Ok o -> okResp o
        | Error e -> errResp e

    let validateName (n: string) =
        if isNull n then Result.Error "Todo name cannot be null"
        else
            let n = n.Trim()
            if n.Length = 0 then Result.Error "Todo name cannot be empty"
            elif n.Length > 100 then Result.Error "Todo name cannot be longer than 100"
            else Name n |> Result.Ok

    [<HttpGet>]
    member __.GetTodos() =
        let todos = repo.All() |> List.map toTodoDto
        ActionResult<TodoDto list>(todos)

    [<HttpGet("{todoId}")>]
    member __.GetTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound toTodoDto

    [<HttpGet("{todoId}/tasks")>]
    member __.GetTasks(todoId: int64) =
        let getTasks todo =
            todo.Tasks |> List.map toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound getTasks

    [<HttpGet("{todoId}/tasks/{taskId}")>]
    member __.GetTask(todoId: int64, taskId: int64) =
        let tryGetTask todo =
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> okOrNotFound toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound tryGetTask

    [<HttpPost>]
    member __.AddTodo( [<FromBody>] name: string) =
        let todoCreated (t: Todo) =
            createdAt "GetTodo" { todoId = t.Id.Value } (toTodoDto t)
        validateName name
        |> Result.map createTodo
        |> Result.map (tee repo.Upsert)
        |> toResponse todoCreated badRequest

    [<HttpPut("{todoId}")>]
    member __.RenameTodo(todoId: int64,  [<FromBody>] name: string) =
        let rename todo =
            validateName name
            |> Result.map (renameTodo todo)
            |> Result.map (tee repo.Upsert)
            |> Result.map toTodoDto
            |> toResponse ok badRequest
        todoId
        |> Id.from
        |> repo.TryGet
        |> Option.map rename
        |> Option.defaultWith notFound

    [<HttpPost("{todoId}/tasks")>]
    member __.AddTask(todoId: int64,  [<FromBody>] title: string) =
        let create todo =
            createTask title
            |> addTask todo
            |> repo.Upsert
            |> noContent
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound create

    [<HttpPatch("{todoId}/tasks/{taskId}")>]
    member __.RenameTask(todoId: int64, taskId: int64,  [<FromBody>] title: string) =
        let tryRenameTodo todo =
            let rename task =
                let renameOrPassthru x =
                    if x = task
                    then renameTask x title
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map renameOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> okOrNotFound rename
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound tryRenameTodo

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member __.ChangeStatus(todoId: int64, taskId: int64,  [<FromQuery>] status: StatusDto) =
        let tryChangeStatus todo =
            let replace (task: Task) =
                let replaceOrPassthru x =
                    if x = task
                    then { x with Status = toStatus status }
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map replaceOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> okOrNotFound replace
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound tryChangeStatus

    [<HttpDelete("{todoId}")>]
    member __.DeleteTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound repo.Remove

    [<HttpDelete("{todoId}/tasks/{taskId}")>]
    member __.DeleteTask(todoId: int64, taskId: int64) =
        let tryDeleteTask todo =
            let delete task =
                let updated = { todo with Tasks = todo.Tasks |> List.except [ task ] }
                repo.Upsert updated
                updated |> toTodoDto
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> okOrNotFound delete
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound tryDeleteTask
