using System;

namespace AllocatrApi.Models;

public class TaskComment
{
    public TaskItem TaskItem { get; set; } = null!;
    public Guid TaskItemId { get; set; }

}
