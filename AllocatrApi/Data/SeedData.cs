using System;
using AllocatrApi.Dtos;

namespace AllocatrApi.Data;

public static class SeedData
{
    public readonly static List<ProjectDto> Projects = new()
    {
        new (
            Guid.NewGuid(),
            // Guid.NewGuid("6842b2f8-1758-8013-bf37-894d525dcc41"),
            "ALC-0001",
            "Home Plumbing Repair",
            "Fix major leaks and replace faulty faucets in a residential property.",
            "home service",
            ["plumber", "plumbing", "fix bathtub"],
            DateTime.Parse("2025-09-01T08:00:00Z"),
            DateTime.Parse("2025-09-10T08:00:00Z"),
            DateOnly.Parse("2025-09-02"),
            DateOnly.Parse("2025-09-12"),
            "active",
            60,
            "high",
            Guid.Parse("1b02ab97-6b20-4b85-8c72-4f8c9e7f4a03"),
            [
                Guid.Parse("c0a1a4d1-4b1f-45a3-8a7a-3a2f5a1c77c1"),
                Guid.Parse("ab23cd45-1234-4ef1-9b7a-99c2f8d5e888"),
            ],
            4,
            12,
            DateTime.Parse("2025-09-09T09:30:00Z"),
            false,
            true,
            250,
            "USD",
            []
        ),

        new (
            Guid.Parse("7d0c8331-3b24-4c7d-b6b7-5b517b92efc5"),
            "ALC-0003",
            "Electrical Wiring Upgrade",
            "Upgrade wiring in an old apartment to meet safety standards.",
            "home service",
            ["electrical service", "electrician"],
            DateTime.Parse("2025-09-05T08:30:00Z"),
            DateTime.Parse("2025-09-15T08:30:00Z"),
            DateOnly.Parse("2025-09-06"),
            DateOnly.Parse("2025-09-20"),
            "pending",
            20,
            "high",
            Guid.Parse("99cf451e-b012-4a51-95a3-3a02cf21a7f3"),
            [
                Guid.Parse("e61791d9-dc24-43ba-b3ad-4a1b8b49c0e1"),
            ],
            3,
            4,
            DateTime.Parse("2025-09-14T12:00:00Z"),
            false,
            true,
            500,
            "USD",
            []
        ),

        // new (
        //     "9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e",
        //     "ALC-0009",
        //     "Allocatr Mobile App",
        //     "Develop the Allocatr mobile app to improve accessibility for both clients and skilled professionals.",
        //     "digital",
        //     ["mobile development", "android developer", "mobile app"],
        //     DateTime.Parse("2025-09-20T08:00:00Z"),
        //     DateTime.Parse("2025-10-02T10:00:00Z"),
        //     DateOnly.Parse("2025-09-22"),
        //     DateOnly.Parse("2025-12-15"),
        //     "active",
        //     40,
        //     "high",
        //     "2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a",
        //     [
        //         "3a4e2e61-29f5-47f7-b12b-6fd2b50c8da3",
        //     ],
        //     4,
        //     18,
        //     DateTime.Parse("2025-10-09T11:30:00Z"),
        //     true,
        //     false,
        //     1800,
        //     "USD",
        //     []
        // ),
        
        // new (
        //     "7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02",
        //     "ALC-0003",
        //     "Door and Lock Replacement",
        //     "Replace broken doors and install new secure locking systems.",
        //     "home service",
        //     ["carpentry", "security"],
        //     DateTime.Parse("2025-09-05T08:00:00Z"),
        //     DateTime.Parse("2025-09-08T16:00:00Z"),
        //     DateOnly.Parse("2025-09-06"),
        //     DateOnly.Parse("2025-09-09"),
        //     "active",
        //     80,
        //     "high",
        //     "e0f11b22-99f1-4b8a-a221-92a8c2f11234",
        //     [
        //         "c91d2e4a-77b3-4a0a-91b2-1f9c6b8e7002"
        //     ],
        //     2,
        //     5,
        //     DateTime.Parse("2025-09-07T11:10:00Z"),
        //     false,
        //     true,
        //     120,
        //     "USD",
        //     []
        // ),
    };
}
