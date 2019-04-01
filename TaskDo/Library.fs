namespace TaskDo


type TaskStatus =
    | Undone
    | PartiallyDone // when some subtasks were done
    | Done

type Repetition =
    | Once
    | Recurring


type TaskData = {
    Title: string
    Status: TaskStatus
}

type Task =
    | Todo of TaskData
    | Subtask

type TaskList = Task list



module Task =
    
    let create title =
        {Title = title; Status = Undone}

