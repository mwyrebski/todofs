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
    Tasks: TaskDto list
    }
and TaskDto = {
    Id: int64
    Title: string
    Status: StatusDto
    }
and StatusDto =
    | Undone = 0
    | Done = 1
and EnvelopeDto = {
    Todos: TodoDto list
    }


type TodoIdParam = { todoId: int64 }
type AddTaskRequest = { title: string; priority: string }
type AddTodoRequest = { name: string; label: string }

module Result =
    let toResponse okResp errResp result =
        match result with
        | Ok o -> okResp o
        | Error e -> errResp e

module Option =
    let toResponse someResp noneResp opt =
        match opt with
        | Some o -> someResp o
        | None -> noneResp()

[<Route("api/[controller]")>]
[<ApiController>]
type TodosController(repo: TodosRepository) as self =
    inherit ControllerBase()
    
    let wrapWithEnvelope todos =
        { Todos = todos }

    let toTaskDto (x: Task) =
        let status =
            match x.Status with
            | Undone -> StatusDto.Undone
            | Done -> StatusDto.Done
        { Id = x.Id.Value; Title = x.Title; Status = status }

    let toTodoDto (x: Todo) =
        { Id = x.Id.Value
          Name = x.Name.Value
          TasksCount = List.length x.Tasks
          Tasks = List.map toTaskDto x.Tasks }

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
    let badRequest (e: obj) = e |> self.BadRequest :> IActionResult
    let ok x = x |> self.Ok :> IActionResult
    let okOrNotFound okFunc option =
        option
        |> Option.map okFunc
        |> Option.toResponse ok notFound

    [<HttpGet>]
    member __.GetTodos() =
        repo.All()
        |> List.map toTodoDto
        |> wrapWithEnvelope
        |> ok

    [<HttpGet("{todoId}")>]
    member __.GetTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound toTodoDto

    [<HttpGet("{todoId}/tasks")>]
    member __.GetTasks(todoId: int64) =
        let getTasks (todo: Todo) =
            todo.Tasks |> List.map toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound getTasks

    [<HttpGet("{todoId}/tasks/{taskId}")>]
    member __.GetTask(todoId: int64, taskId: int64) =
        let tryGetTask (todo: Todo) =
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> okOrNotFound toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound tryGetTask

    [<HttpPost>]
    member __.AddTodo( [<FromBody>] request) =
        let todoCreated (t: Todo) =
            createdAt "GetTodo" { todoId = t.Id.Value } (toTodoDto t)
        Name.create request.name
        |> Result.bind (fun n ->
            Label.create request.label
            |> Result.bind
                   (fun l -> createTodo n
                             |> addLabelTodo l
                             |> Ok))
        |> Result.map (tee repo.Upsert)
        |> Result.toResponse todoCreated badRequest

    [<HttpPut("{todoId}")>]
    member __.RenameTodo(todoId: int64,  [<FromBody>] name: string) =
        let rename todo =
            Name.create name
            |> Result.map (renameTodo todo)
            |> Result.map (tee repo.Upsert)
            |> Result.map toTodoDto
            |> Result.toResponse ok badRequest
        todoId
        |> Id.from
        |> repo.TryGet
        |> Option.map rename
        |> Option.defaultWith notFound

    [<HttpPost("{todoId}/tasks")>]
    member __.AddTask(todoId: int64,  [<FromBody>] request) =
        let create todo =
            createTask request.title
            |> addTask todo
            |> repo.Upsert
            |> noContent
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound create

    [<HttpPatch("{todoId}/tasks/{taskId}")>]
    member __.RenameTask(todoId: int64, taskId: int64,  [<FromBody>] title: string) =
        let tryRenameTodo (todo: Todo) =
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
        |> Option.map tryRenameTodo
        |> Option.defaultWith notFound

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member __.ChangeStatus(todoId: int64, taskId: int64,  [<FromQuery>] status: StatusDto) =
        let tryChangeStatus (todo: Todo) =
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
        |> Option.map tryChangeStatus
        |> Option.defaultWith notFound

    [<HttpDelete("{todoId}")>]
    member __.DeleteTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> okOrNotFound (fun x -> repo.Remove x |> ignore; self.Ok())

    [<HttpDelete("{todoId}/tasks/{taskId}")>]
    member __.DeleteTask(todoId: int64, taskId: int64) =
        let tryDeleteTask (todo: Todo) =
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
        |> Option.map tryDeleteTask
        |> Option.defaultWith notFound
