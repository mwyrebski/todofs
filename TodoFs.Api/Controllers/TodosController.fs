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

    [<HttpGet>]
    member this.GetTodoLists() =
        let todos = repo.All() |> List.map toTodoDto
        ActionResult<TodoDto list>(todos)

    [<HttpGet("{todoId}")>]
    member this.GetTodoList(todoId: int64) =
        let todoId = Id.from todoId
        let todo = repo.TryGet todoId
        match todo with
        | Some value -> toTodoDto value |> this.Ok :> IActionResult
        | None -> this.NotFound() :> IActionResult

    [<HttpGet("{todoId}/tasks")>]
    member this.GetTasks(todoId: int64) =
        let todoId = Id.from todoId
        let todo = repo.TryGet todoId
        match todo with
        | Some value -> List.map toTaskDto value.Tasks |> this.Ok :> IActionResult
        | None -> this.NotFound() :> IActionResult

    [<HttpGet("{todoId}/tasks/{taskId}")>]
    member this.GetTask(todoId: int64, taskId: int64) =
        let todoId = Id.from todoId
        let taskId = Id.from taskId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let taskOpt = List.tryFind (fun (x: Task) -> x.Id = taskId) todo.Tasks
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
        let todoId = Id.from todoId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let renamed = renameTodo todo name
            repo.Upsert renamed
            this.Ok() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPost("{todoId}/tasks")>]
    member this.AddTask(todoId: int64,  [<FromBody>] title: string) =
        let todoId = Id.from todoId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let task = createTask title
            let newTodo = addTask todo task
            repo.Upsert newTodo
            this.NoContent() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member this.RenameTask(todoId: int64, taskId: int64,  [<FromBody>] title: string) =
        let todoId = Id.from todoId
        let taskId = Id.from taskId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let taskOpt = List.tryFind (fun (x: Task) -> x.Id = taskId) todo.Tasks
            match taskOpt with
            | Some task ->
                let newTasks = List.map (fun (x:Task) -> if x.Id = taskId then renameTask x title else x) todo.Tasks
                let newTodo = {todo with Tasks = newTasks}
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpPut("{todoId}/tasks/{taskId}")>]
    member this.ChangeStatus(todoId: int64, taskId: int64,  [<FromQuery>] status: StatusDto) =
        let todoId = Id.from todoId
        let taskId = Id.from taskId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let taskOpt = List.tryFind (fun (x: Task) -> x.Id = taskId) todo.Tasks
            match taskOpt with
            | Some task ->
                let status = toStatus status
                let newTasks = List.map (fun (x:Task) -> if x.Id = taskId then {x with Status = status} else x) todo.Tasks
                let newTodo = {todo with Tasks = newTasks}
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult

    [<HttpDelete("{todoId}")>]
    member this.DeleteTodoList(todoId: int64) =
        let todoId = Id.from todoId
        let removed = repo.Remove todoId
        if removed then
            this.Ok() :> IActionResult
        else
            this.NotFound() :> IActionResult

    [<HttpDelete("{todoId}/tasks/{taskId}")>]
    member this.DeleteTask(todoId: int64, taskId: int64) =
        let todoId = Id.from todoId
        let taskId = Id.from taskId
        let todoOpt = repo.TryGet todoId
        match todoOpt with
        | Some todo ->
            let taskOpt = List.tryFind (fun (x: Task) -> x.Id = taskId) todo.Tasks
            match taskOpt with
            | Some task ->
                let newTasks = List.except [task] todo.Tasks
                let newTodo = {todo with Tasks = newTasks}
                repo.Upsert newTodo
                toTodoDto newTodo |> this.Ok :> IActionResult
            | None -> this.NotFound() :> IActionResult
        | None ->
            this.NotFound() :> IActionResult
