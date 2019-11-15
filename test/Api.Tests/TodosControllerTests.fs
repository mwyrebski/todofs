module TodoFs.Api.TodosController.Tests

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Caching.Memory
open Swensen.Unquote
open TodoFs.Api.Controllers
open TodoFs.Api.Data
open TodoFs.Domain
open TodoFs.Domain.Implementation
open Xunit


let repo = new TodosRepository(new MemoryCache(MemoryCacheOptions()))
let sut = TodosController repo


module GetTodos =

    let private extractTodos (actionResult: IActionResult) =
        let result = actionResult :?> OkObjectResult
        let envelope = result.Value :?> EnvelopeDto
        envelope.Todos

    [<Fact>]
    let ``Returns OkObjectResult with EnvelopeDto with empty value``() =
        let todos = sut.GetTodos() |> extractTodos

        todos =! []

    [<Fact>]
    let ``Returns expected Todo from GetTodos``() =
        let todo = Name "todo1" |> createTodo
        todo |> repo.Upsert

        let todos = sut.GetTodos() |> extractTodos

        List.length todos =! 1
        let head = List.head todos
        head.Id =! todo.Id.Value
        head.Name =! todo.Name.Value
        head.Tasks =! []

