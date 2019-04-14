module Tests

open System
open Xunit
open FsUnit.Xunit
open TodoFs
open TodoFs.Implementation

[<Fact>]
let ``createTask should create Task in status Undone`` () =
    let actual = createTask "any-title"

    actual.Status |> should equal Undone

[<Fact>]
let ``renameTask should return new Task with changed Title`` () =
    let task = createTask "any-title"
    let expected = "expected-title"

    let actual = renameTask task expected

    actual.Title |> should equal expected

[<Fact>]
let ``doTask should return new Task with status Done`` () =
    let task = createTask "any-title"

    let actual = doTask task

    actual.Status |> should equal Done

[<Fact>]
let ``createTodo should create Todo list with empty Tasks`` () =
    let actual = createTodo "any-name"

    actual.Tasks |> should be Empty

[<Fact>]
let ``addTask should return new TodoList with expected task added`` () =
    let todo = createTodo "any-name"
    let task = createTask "any-title"

    let actual = addTask todo task

    actual.Tasks |> should contain task

[<Fact>]
let ``addTasks should return new TodoList with expected tasks added`` () =
    let todo = createTodo "any-name"
    let task1 = createTask "any-title"
    let task2 = createTask "any-title"

    let actual = addTasks todo [task1; task2]

    actual.Tasks |> should contain task1
    actual.Tasks |> should contain task2

[<Fact>]
let ``markDone should return new TodoList with specific task with status Done`` () =
    let expected = createTask "some-title"
    let todoList = addTasks (createTodo "any-name") [
            createTask "any-title-1"
            expected
            createTask "any-title-2"
        ]

    let actual = markDone todoList expected

    actual.Tasks.[0].Status |> should equal Undone
    actual.Tasks.[1].Status |> should equal Done
    actual.Tasks.[2].Status |> should equal Undone

[<Fact>]
let ``toString should generate expected string representation`` () =
    let todoList = addTasks (createTodo "any-name") [
            createTask "any-title-1"
            createTask "any-title-2" |> doTask
            createTask "any-title-3"
        ]
    let expected = String.concat Environment.NewLine [
        ""
        "..:: any-name ::.."
        "------------------"
        "[ ] any-title-1"
        "[x] any-title-2"
        "[ ] any-title-3"
        ""
        ]

    let actual = toString todoList

    actual |> should equal expected


