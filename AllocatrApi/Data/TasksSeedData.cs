using System;
using AllocatrApi.Dtos;

namespace AllocatrApi.Data;

public class TasksSeedData
{
    public readonly static List<TaskDto> Tasks =
    [
        new TaskDto(
            Guid.Parse("b1111111-1111-1111-1111-111111111111"),
            "Measure door frames",
            "Measure all door frames for accurate fitting.",
            "complete",
            "normal",
            1,
            DateTime.Parse("2025-09-05T09:00:00Z"),
            DateOnly.Parse("2025-09-05"),
            null,
            DateTime.Parse("2025-09-05T10:00:00Z"),
            null,
            null,
            Guid.Parse("7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02")
        ),

        new TaskDto(
            Guid.Parse("b2222222-2222-2222-2222-222222222222"),
            "Remove old doors",
            "Carefully remove damaged doors and frames.",
            "complete",
            "high",
            2,
            DateTime.Parse("2025-09-05T10:30:00Z"),
            DateOnly.Parse("2025-09-06"),
            null,
            DateTime.Parse("2025-09-06T12:00:00Z"),
            null,
            null,
            Guid.Parse("7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02")
        ),

        new TaskDto(
            Guid.Parse("b3333333-3333-3333-3333-333333333333"),
            "Install new doors",
            "Fit and align new doors.",
            "active",
            "high",
            3,
            DateTime.Parse("2025-09-06T13:00:00Z"),
            DateOnly.Parse("2025-09-07"),
            null,
            null,
            null,
            null,
            Guid.Parse("7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02")
        ),

        new TaskDto(
            Guid.Parse("b4444444-4444-4444-4444-444444444444"),
            "Install locks and test",
            "Install locks and test for smooth operation.",
            "pending",
            "high",
            4,
            DateTime.Parse("2025-09-07T14:00:00Z"),
            DateOnly.Parse("2025-09-08"),
            null,
            null,
            null,
            null,
            Guid.Parse("7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02")
        ),
    ];
}
