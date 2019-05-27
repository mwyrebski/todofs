module TodosControllerTests

open Xunit
open FsUnit.Xunit
open TodoFs.Implementation
open TodoFs.Api.Controllers
open TodoFs.Api.Data
open Microsoft.Extensions.Caching.Memory
open Microsoft.AspNetCore.Mvc


let setup() = 
    let repo = new TodosRepository(new MemoryCache(new MemoryCacheOptions()))
    let controller = new TodosController(repo)
    repo, controller


module GetTodo =
    let repo, tested = setup()

    [<Fact>]
    let ``Given no data, GetTodo returns NotFound`` () =
        let actual = tested.GetTodo 0L

        actual |> should be instanceOfType<NotFoundResult>

    [<Fact>]
    let ``Given single todo, GetTodo returns Ok`` () =
        Name.create "any-name"
        |> Result.map createTodo
        |> Result.map repo.Upsert |> ignore

        let actual = tested.GetTodo 1L

        actual |> should be instanceOfType<OkObjectResult>
        
module GetTasks =
    let repo, tested = setup()

    [<Fact>]
    let ``Given no todo, GetTasks returns NotFound`` () =
        let actual = tested.GetTasks 0L

        actual |> should be instanceOfType<NotFoundResult>

    [<Fact>]
    let ``Given todo without tasks, GetTasks returns empty list`` () =
        Name.create "any-name"
        |> Result.map createTodo
        |> Result.map repo.Upsert |> ignore

        let actual = tested.GetTasks 1L

        actual |> should be instanceOfType<OkObjectResult>
        (actual :?> OkObjectResult).Value |> should be Empty


module GetTask =
    let repo, tested = setup()

    [<Fact>]
    let ``Given no data, returns NotFound`` () =
        let actual = tested.GetTask(0L, 0L)

        actual |> should be instanceOfType<NotFoundResult>

    [<Fact>]
    let ``Given single todo but no tasks, returns NotFound`` () =
        Name.create "any-name"
        |> Result.map createTodo
        |> Result.map repo.Upsert |> ignore

        let actual = tested.GetTask(1L, -1L)

        actual |> should be instanceOfType<NotFoundResult>
   
    [<Fact>]
    let ``Given single todo and a task, returns Ok`` () =
        Name.create "any-name"
        |> Result.map createTodo
        |> Result.map (fun t -> addTask t (createTask "any-task"))
        |> Result.map repo.Upsert |> ignore

        let actual = tested.GetTask(1L, 1L)

        actual |> should be instanceOfType<OkObjectResult>
   