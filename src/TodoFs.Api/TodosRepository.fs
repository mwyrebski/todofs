namespace TodoFs.Api.Data

open System
open Microsoft.Extensions.Caching.Memory
open TodoFs

type TodosRepository(cache: IMemoryCache) =

    [<Literal>]
    let cacheKey = "todos-repository"

    let data(): Todo list =
        cache.GetOrCreate(cacheKey, (fun _ -> []))

    let save (value: Todo list) =
        cache.Set(cacheKey, value)

    member this.All() = data()

    member this.TryGet id: Todo option =
        data() |> List.tryFind (fun x -> x.Id = id)

    member this.Upsert(todo: Todo) =
        let predicate x = x.Id = todo.Id
        let replace t =
            if predicate t
            then todo
            else t
        let data = data()
        let exists = List.exists predicate data
        match exists with
        | true -> data |> List.map replace
        | false -> todo :: data
        |> save
        |> ignore

    member this.Remove todo =
        data()
        |> List.except [ todo ]
        |> save

    interface IDisposable with
        member this.Dispose() =
            cache.Dispose()
