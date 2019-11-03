module TodoFs.Api.TodosController.Tests

open Microsoft.AspNetCore.Mvc
open TodoFs.Api.Controllers
open Microsoft.Extensions.Caching.Memory
open TodoFs.Api.Data
open Xunit
open FsUnit.Xunit


let repo = new TodosRepository(new MemoryCache(MemoryCacheOptions()))


[<Fact>]
let ``Returns OkObjectResult with EnvelopeDto from GetTodos``() =
    let sut = TodosController repo

    let actual = sut.GetTodos()

    actual |> should be ofExactType<OkObjectResult>
    (actual :?> OkObjectResult).Value |> should be ofExactType<EnvelopeDto>
