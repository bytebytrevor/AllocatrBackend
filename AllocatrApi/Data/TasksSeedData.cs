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
        
        new TaskDto(
            Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            "Initial plumbing inspection",
            "Inspect kitchen and bathroom plumbing for leaks, pressure issues, and faulty fittings.",
            "complete",
            "high",
            1,
            DateTime.Parse("2025-09-01T09:00:00Z"),
            DateOnly.Parse("2025-09-02"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("a2222222-2222-2222-2222-222222222222"),
            "Repair leaking kitchen sink pipe",
            "Replace damaged pipe joints and reseal under-sink connections.",
            "active",
            "high",
            2,
            DateTime.Parse("2025-09-02T12:00:00Z"),
            DateOnly.Parse("2025-09-04"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("a3333333-3333-3333-3333-333333333333"),
            "Replace faulty bathroom faucet",
            "Remove old faucet and install new mixer tap in main bathroom.",
            "active",
            "medium",
            3,
            DateTime.Parse("2025-09-03T08:30:00Z"),
            DateOnly.Parse("2025-09-05"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("a4444444-4444-4444-4444-444444444444"),
            "Fix toilet flushing mechanism",
            "Repair or replace faulty flush valve causing continuous water flow.",
            "pending",
            "medium",
            4,
            DateTime.Parse("2025-09-04T10:00:00Z"),
            DateOnly.Parse("2025-09-06"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("a5555555-5555-5555-5555-555555555555"),
            "Pressure test repaired pipes",
            "Run pressure tests on all repaired plumbing lines to confirm no leaks.",
            "pending",
            "low",
            5,
            DateTime.Parse("2025-09-07T09:00:00Z"),
            DateOnly.Parse("2025-09-08"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("a6666666-6666-6666-6666-666666666666"),
            "Final cleanup and client walkthrough",
            "Clean work areas and explain completed repairs to the homeowner.",
            "pending",
            "low",
            6,
            DateTime.Parse("2025-09-09T08:30:00Z"),
            DateOnly.Parse("2025-09-09"),
            null,
            null,
            null,
            null,
            Guid.Parse("6842b2f8-1758-8013-bf37-894d525dcc41")
        ),

        new TaskDto(
            Guid.Parse("c1111111-aaaa-4a11-b111-111111111111"),
            "Electrical safety inspection",
            "Inspect existing wiring, sockets, and breaker panel for safety and compliance issues.",
            "complete",
            "high",
            1,
            DateTime.Parse("2025-09-05T09:00:00Z"),
            DateOnly.Parse("2025-09-06"),
            DateTime.Parse("2025-09-06T11:00:00Z"),
            DateTime.Parse("2025-09-06T11:00:00Z"),
            [],
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5")
        ),

        new TaskDto(
            Guid.Parse("c2222222-bbbb-4b22-b222-222222222222"),
            "Replace outdated wiring",
            "Remove old aluminum wiring and install modern copper wiring throughout the apartment.",
            "active",
            "high",
            2,
            DateTime.Parse("2025-09-06T08:30:00Z"),
            DateOnly.Parse("2025-09-10"),
            DateTime.Parse("2025-09-08T14:00:00Z"),
            DateTime.Parse("2025-09-10T17:00:00Z"),
            [],
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5")
        ),

        new TaskDto(
            Guid.Parse("c3333333-cccc-4c33-b333-333333333333"),
            "Upgrade circuit breaker panel",
            "Install a new breaker panel with modern safety breakers and proper labeling.",
            "pending",
            "high",
            3,
            DateTime.Parse("2025-09-09T10:00:00Z"),
            DateOnly.Parse("2025-09-12"),
            DateTime.Parse("2025-09-12T16:00:00Z"),
            DateTime.Parse("2025-09-12T16:00:00Z"),
            [],
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5")
        ),

        new TaskDto(
            Guid.Parse("c4444444-dddd-4d44-b444-444444444444"),
            "Install additional power outlets",
            "Add new grounded power outlets in bedrooms and living areas as per safety code.",
            "pending",
            "medium",
            4,
            DateTime.Parse("2025-09-11T09:00:00Z"),
            DateOnly.Parse("2025-09-14"),
            DateTime.Parse("2025-09-14T15:00:00Z"),
            DateTime.Parse("2025-09-14T15:00:00Z"),
            [],
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5")
        ),

        new TaskDto(
            Guid.Parse("c5555555-eeee-4e55-b555-555555555555"),
            "Final electrical testing and sign-off",
            "Test all circuits, outlets, and breakers and prepare compliance sign-off.",
            "pending",
            "medium",
            5,
            DateTime.Parse("2025-09-14T08:30:00Z"),
            DateOnly.Parse("2025-09-15"),
            DateTime.Parse("2025-09-15T12:00:00Z"),
            DateTime.Parse("2025-09-15T12:00:00Z"),
            [],
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5")
        ),

        new TaskDto(
            Guid.Parse("a1111111-1111-4a11-a111-111111111111"),
            "Define mobile app requirements",
            "Finalize functional requirements for client and skilled worker mobile experiences.",
            "complete",
            "high",
            1,
            DateTime.Parse("2025-09-20T09:00:00Z"),
            DateOnly.Parse("2025-09-22"),
            DateTime.Parse("2025-09-22T16:30:00Z"),
            DateTime.Parse("2025-09-22T16:30:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a2222222-2222-4b22-a222-222222222222"),
            "Design mobile UI/UX",
            "Create wireframes and high-fidelity designs for core mobile screens.",
            "complete",
            "high",
            2,
            DateTime.Parse("2025-09-22T10:00:00Z"),
            DateOnly.Parse("2025-09-27"),
            DateTime.Parse("2025-09-27T15:00:00Z"),
            DateTime.Parse("2025-09-27T15:00:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a3333333-3333-4c33-a333-333333333333"),
            "Set up Flutter project",
            "Initialize Flutter project, configure environments, and integrate base dependencies.",
            "active",
            "high",
            3,
            DateTime.Parse("2025-09-25T08:30:00Z"),
            DateOnly.Parse("2025-09-30"),
            DateTime.Parse("2025-09-28T14:00:00Z"),
            DateTime.Parse("2025-09-30T17:00:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a4444444-4444-4d44-a444-444444444444"),
            "Implement authentication flow",
            "Build login, registration, and role-based access using the existing API.",
            "active",
            "high",
            4,
            DateTime.Parse("2025-09-28T09:00:00Z"),
            DateOnly.Parse("2025-10-05"),
            DateTime.Parse("2025-10-02T16:00:00Z"),
            DateTime.Parse("2025-10-05T18:00:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a5555555-5555-4e55-a555-555555555555"),
            "Integrate project and task APIs",
            "Consume backend endpoints for projects, tasks, and status updates.",
            "pending",
            "medium",
            5,
            DateTime.Parse("2025-10-03T08:00:00Z"),
            DateOnly.Parse("2025-10-12"),
            DateTime.Parse("2025-10-12T17:00:00Z"),
            DateTime.Parse("2025-10-12T17:00:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a6666666-6666-4f66-a666-666666666666"),
            "Testing and performance optimization",
            "Test app on Android and iOS devices and optimize performance.",
            "pending",
            "medium",
            6,
            DateTime.Parse("2025-10-10T09:00:00Z"),
            DateOnly.Parse("2025-10-18"),
            DateTime.Parse("2025-10-18T16:30:00Z"),
            DateTime.Parse("2025-10-18T16:30:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),

        new TaskDto(
            Guid.Parse("a7777777-7777-4a77-a777-777777777777"),
            "Prepare release build",
            "Create production builds and prepare store submission assets.",
            "pending",
            "low",
            7,
            DateTime.Parse("2025-10-15T10:00:00Z"),
            DateOnly.Parse("2025-10-20"),
            DateTime.Parse("2025-10-20T14:00:00Z"),
            DateTime.Parse("2025-10-20T14:00:00Z"),
            [],
            Guid.Parse("2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a"),
            Guid.Parse("9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e")
        ),
        
    ];
}
