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
        let mutable replaced = false
        let tryReplace (d: Todo) =
            if d.Id = todo.Id
            then
                replaced <- true
                todo
            else
                d
        let newData = List.map tryReplace data
        if replaced then
            data <- newData
        else
            data <- todo :: data

    member this.Remove id =
        let filtered = List.filter (fun x -> x.Id <> id) data
        if data <> filtered then
            data <- filtered
            true
        else 
            false

    interface IDisposable with
        member this.Dispose() = cache.Dispose()


