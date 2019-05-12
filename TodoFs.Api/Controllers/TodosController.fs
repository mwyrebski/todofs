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

    let toTodoDto (x: Todo) =
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
    member private this.notFound() = this.NotFound() :> IActionResult
    member private this.noContent() = this.NoContent() :> IActionResult
    member private this.okOrNotFound f o =
        match o with
        | Some x -> f x |> this.ok
        | None -> this.notFound()

    [<HttpGet>]
    member this.GetTodos() =
        let todos = repo.All() |> List.map toTodoDto
        ActionResult<TodoDto list>(todos)

    [<HttpGet("{todoId}")>]
    member this.GetTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound toTodoDto

    [<HttpGet("{todoId}/tasks")>]
    member this.GetTasks(todoId: int64) =
        let getTasks todo =
            todo.Tasks |> List.map toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound getTasks

    [<HttpGet("{todoId}/tasks/{taskId}")>]
    member this.GetTask(todoId: int64, taskId: int64) =
        let tryGetTask todo =
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> this.okOrNotFound toTaskDto
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound tryGetTask

    [<HttpPost>]
    member this.AddTodo( [<FromBody>] name: string) =
        let todo = createTodo name
        repo.Upsert todo
        this.CreatedAtAction("GetTodo", { todoId = todo.Id.Value }, todo |> toTodoDto)

    [<HttpPut("{todoId}")>]
    member this.RenameTodo(todoId: int64,  [<FromBody>] name: string) =
        let rename todo =
            renameTodo todo name
            |> repo.Upsert
            |> this.ok
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound rename

    [<HttpPost("{todoId}/tasks")>]
    member this.AddTask(todoId: int64,  [<FromBody>] title: string) =
        let create todo =
            createTask title
            |> addTask todo
            |> repo.Upsert
            |> this.noContent
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound create

    [<HttpPatch("{todoId}/tasks/{taskId}")>]
    member this.RenameTask(todoId: int64, taskId: int64,  [<FromBody>] title: string) =
        let tryRenameTodo todo =
            let rename task =
                let renameOrPassthru x =
                    if x = task
                    then renameTask x title
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map renameOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo |> this.ok
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> this.okOrNotFound rename
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound tryRenameTodo

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member this.ChangeStatus(todoId: int64, taskId: int64,  [<FromQuery>] status: StatusDto) =
        let tryChangeStatus todo =
            let replace (task: Task) =
                let replaceOrPassthru x =
                    if x = task
                    then { x with Status = toStatus status }
                    else x
                let newTodo = { todo with Tasks = todo.Tasks |> List.map replaceOrPassthru }
                repo.Upsert newTodo
                toTodoDto newTodo |> this.ok
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> this.okOrNotFound replace
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound tryChangeStatus

    [<HttpDelete("{todoId}")>]
    member this.DeleteTodo(todoId: int64) =
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound repo.Remove

    [<HttpDelete("{todoId}/tasks/{taskId}")>]
    member this.DeleteTask(todoId: int64, taskId: int64) =
        let tryDeleteTask todo =
            let delete task =
                let updated = { todo with Tasks = todo.Tasks |> List.except [ task ] }
                repo.Upsert updated
                updated |> toTodoDto |> this.ok
            todo.Tasks
            |> tryFindTask (Id.from taskId)
            |> this.okOrNotFound delete
        todoId
        |> Id.from
        |> repo.TryGet
        |> this.okOrNotFound tryDeleteTask
