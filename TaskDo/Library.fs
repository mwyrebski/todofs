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

type TaskList = {
    Name: string
    Tasks: Task list
    }


module TaskList =
    
    let create name =
        {Name = name; Tasks = []}

    let addTask task tasklist =
        {tasklist with Tasks = task :: tasklist.Tasks}


module Task =

    let create title =
        {Title = title; Status = Undone}

    let changeTitle title task =
        {task with Title = title}

    let markDone task =
        {task with Status = Done}

