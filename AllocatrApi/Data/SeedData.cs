using System;
using AllocatrApi.Dtos;

namespace AllocatrApi.Data;

public static class SeedData
{
    public readonly static List<ProjectDto> Projects = new()
    {
        new (
            Guid.NewGuid().ToString(),
            // "6842b2f8-1758-8013-bf37-894d525dcc41",
            "ALC-0001",
            "Home Plumbing Repair",
            "Fix major leaks and replace faulty faucets in a residential property.",
            "home service",
            "plumbing",
            DateTime.Parse("2025-09-01T08:00:00Z"),
            DateTime.Parse("2025-09-10T08:00:00Z"),
            DateOnly.Parse("2025-09-02"),
            DateOnly.Parse("2025-09-12"),
            "active",
            60,
            "high",
            "1b02ab97-6b20-4b85-8c72-4f8c9e7f4a03",
            [
                "c0a1a4d1-4b1f-45a3-8a7a-3a2f5a1c77c1",
                "ab23cd45-1234-4ef1-9b7a-99c2f8d5e888",
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
            "7d0c8331-3b24-4c7d-b6b7-5b517b92efc5",
            "ALC-0003",
            "Electrical Wiring Upgrade",
            "Upgrade wiring in an old apartment to meet safety standards.",
            "home service",
            "electrical",
            DateTime.Parse("2025-09-05T08:30:00Z"),
            DateTime.Parse("2025-09-15T08:30:00Z"),
            DateOnly.Parse("2025-09-06"),
            DateOnly.Parse("2025-09-20"),
            "pending",
            20,
            "high",
            "99cf451e-b012-4a51-95a3-3a02cf21a7f3",
            [
                "e61791d9-dc24-43ba-b3ad-4a1b8b49c0e1",
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

        new (
            "9c22f4c1-3f2b-4b59-bd9d-7c0e45a04a9e",
            "ALC-0009",
            "Allocatr Mobile App",
            "Develop the Allocatr mobile app to improve accessibility for both clients and skilled professionals.",
            "digital",
            "mobile development",
            DateTime.Parse("2025-09-20T08:00:00Z"),
            DateTime.Parse("2025-10-02T10:00:00Z"),
            DateOnly.Parse("2025-09-22"),
            DateOnly.Parse("2025-12-15"),
            "active",
            40,
            "high",
            "2b32f85e-9b51-4a8f-a52c-2a5f7e3db32a",
            [
                "3a4e2e61-29f5-47f7-b12b-6fd2b50c8da3",
            ],
            4,
            18,
            DateTime.Parse("2025-10-09T11:30:00Z"),
            true,
            false,
            1800,
            "USD",
            []
        ),

        new (
            "9b3a7f12-4c88-4e6a-b2d1-91f3a7e12c11",
            "ALC-0002",
            "House Electrical Rewiring",
            "Replace outdated wiring, install new sockets, and ensure electrical safety compliance.",
            "home service",
            "electrical",
            DateTime.Parse("2025-09-03T08:00:00Z"),
            DateTime.Parse("2025-09-15T17:00:00Z"),
            DateOnly.Parse("2025-09-04"),
            DateOnly.Parse("2025-09-16"),
            "active",
            45,
            "urgent",
            "6f4d2c77-1e8a-4a8f-9b8f-2a3d6c4e9b21",
            [
                "8d1f22c3-9a12-4f7a-b8f1-32a9f88c1001"
            ],
            3,
            9,
            DateTime.Parse("2025-09-10T14:20:00Z"),
            false,
            true,
            180,
            "USD",
            []
        ),

        new (
            "7f8a1b32-98f4-4f1c-9b9e-12e7a4f99b02",
            "ALC-0003",
            "Door and Lock Replacement",
            "Replace broken doors and install new secure locking systems.",
            "home service",
            "carpentry",
            DateTime.Parse("2025-09-05T08:00:00Z"),
            DateTime.Parse("2025-09-08T16:00:00Z"),
            DateOnly.Parse("2025-09-06"),
            DateOnly.Parse("2025-09-09"),
            "active",
            80,
            "high",
            "e0f11b22-99f1-4b8a-a221-92a8c2f11234",
            [
                "c91d2e4a-77b3-4a0a-91b2-1f9c6b8e7002"
            ],
            2,
            5,
            DateTime.Parse("2025-09-07T11:10:00Z"),
            false,
            true,
            120,
            "USD",
            []
        ),

        new (
            "4e91c9a2-6d55-4d8f-bc71-88a72a5c2203",
            "ALC-0004",
            "Boundary Wall Repair",
            "Repair cracked bricks and reinforce a damaged boundary wall.",
            "construction",
            "masonry",
            DateTime.Parse("2025-09-07T07:30:00Z"),
            DateTime.Parse("2025-09-14T17:00:00Z"),
            DateOnly.Parse("2025-09-08"),
            DateOnly.Parse("2025-09-15"),
            "active",
            30,
            "normal",
            "a1d22f9e-4c7a-4f1b-b2a3-99c3f8110099",
            [
                "2c6a8f1e-0d2c-4f99-b981-7c2d11890003"
            ],
            5,
            14,
            DateTime.Parse("2025-09-12T10:00:00Z"),
            false,
            false,
            300,
            "USD",
            []
        ),

        new (
            "c7f2a8b1-11d4-4b99-a7b1-5a2d3f9e3304",
            "ALC-0005",
            "Post Construction Cleanup",
            "Full cleanup after renovation including debris removal and deep cleaning.",
            "home service",
            "cleaning",
            DateTime.Parse("2025-09-10T08:00:00Z"),
            DateTime.Parse("2025-09-11T15:00:00Z"),
            DateOnly.Parse("2025-09-11"),
            DateOnly.Parse("2025-09-12"),
            "active",
            90,
            "normal",
            "b8c12d99-88f1-4a22-b2e9-118c9a771111",
            [
                "f4c2d9a7-5512-4f8a-9c9a-11a7c9900004"
            ],
            1,
            3,
            DateTime.Parse("2025-09-11T14:00:00Z"),
            false,
            true,
            75,
            "USD",
            []
        ),

    };
}
