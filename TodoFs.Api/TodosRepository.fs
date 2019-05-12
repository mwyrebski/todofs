namespace TodoFs.Api.Data

open System
open Microsoft.Extensions.Caching.Memory
open TodoFs

type TodosRepository(cache: IMemoryCache) =

    let mutable data: Todo list = []

    member this.All() = data

    member this.TryGet id: Todo option =
        List.tryFind (fun x -> x.Id = id) data

    member this.Upsert(todo: Todo) =
        let predicate x = x.Id = todo.Id
        let replace t =
            if predicate t
            then todo
            else t
        let exists = List.exists predicate data
        match exists with
        | true -> data <- List.map replace data
        | false -> data <- todo :: data

    member this.Remove todo =
        data <- List.except [ todo ] data

    interface IDisposable with
        member this.Dispose() =
            cache.Dispose()
