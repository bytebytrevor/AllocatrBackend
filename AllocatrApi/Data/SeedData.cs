using System;
using AllocatrApi.Dtos;

namespace AllocatrApi.Data;

public static class SeedData
{
    public readonly static List<ProjectDto> Projects = new()
    {
        new (
            "6842b2f8-1758-8013-bf37-894d525dcc41",
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
            new List<ProjectMemberDto>
            {
                new("c0a1a4d1-4b1f-45a3-8a7a-3a2f5a1c77c1", "Lead Plumber", true, DateTime.Parse("2025-02-02")),
                new("ab23cd45-1234-4ef1-9b7a-99c2f8d5e888", "Apprentice", false, DateTime.Parse("2025-02-03"))
            },
            4,
            12,
            DateTime.Parse("2025-09-09T09:30:00Z"),
            false,
            true,
            250,
            "USD",
            new string[]{}
        ),

        new (
            "9c43e8f9-7d02-48e3-92c7-8c16d5437a60",
            "ALC-0002",
            "Website Redesign",
            "Modernize an outdated corporate website to improve user experience and SEO.",
            "digital",
            "web development",
            DateTime.Parse("2025-09-03T10:00:00Z"),
            DateTime.Parse("2025-09-15T10:00:00Z"),
            DateOnly.Parse("2025-09-04"),
            DateOnly.Parse("2025-10-10"),
            "active",
            40,
            "medium",
            "6e0f0d49-1f53-4211-b0cc-9f2b1c6d22a7",
            new List<ProjectMemberDto>
            {
                new("7aa3db18-4f83-4a10-a11e-0f2e8cb2b1b3", "Frontend Developer", true, DateTime.Parse("2025-02-02")),
                new("b3e6d914-985d-4b3b-9418-06b2f1a74fd9", "UI/UX Designer", false, DateTime.Parse("2025-02-04")),
                new("fa71d249-5e8c-41a7-80c0-3e5c11f2d6e8", "SEO Specialist", false, DateTime.Parse("2025-02-05"))
            },
            5,
            9,
            DateTime.Parse("2025-09-14T15:00:00Z"),
            true,
            false,
            1200,
            "USD",
            new string[]{}
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
            new List<ProjectMemberDto>
            {
                new("e61791d9-dc24-43ba-b3ad-4a1b8b49c0e1", "Electrician", true, DateTime.Parse("2025-02-02")),
            },
            3,
            4,
            DateTime.Parse("2025-09-14T12:00:00Z"),
            false,
            true,
            500,
            "USD",
            new string[]{}
        ),

        new (
            "f8d1b7a9-32b5-49d1-8b29-2dca5baf60a4",
            "ALC-0004",
            "Mobile App for Local Grocer",
            "Develop a mobile ordering app for a neighborhood grocery store.",
            "digital",
            "mobile development",
            DateTime.Parse("2025-09-10T09:00:00Z"),
            DateTime.Parse("2025-09-20T09:00:00Z"),
            DateOnly.Parse("2025-09-11"),
            DateOnly.Parse("2025-10-25"),
            "active",
            45,
            "urgent",
            "27d11e2e-63f1-4d12-9f62-6d2bfb26dbd0",
            new List<ProjectMemberDto>
            {
                new("3b82d613-74b3-4625-aac5-4f2fd5a1cce3", "Mobile Developer", true, DateTime.Parse("2025-02-02")),
                new("1d4e6f27-5a2d-44fc-a12f-6e7a4c3f1a10", "UI Designer", false, DateTime.Parse("2025-02-03"))
            },
            5,
            15,
            DateTime.Parse("2025-09-18T11:00:00Z"),
            true,
            false,
            2800,
            "USD",
            new string[]{}
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
            new List<ProjectMemberDto>
            {
                new("3a4e2e61-29f5-47f7-b12b-6fd2b50c8da3", "Mobile Developer", true, DateTime.Parse("2025-02-02")),
            },
            4,
            18,
            DateTime.Parse("2025-10-09T11:30:00Z"),
            true,
            false,
            1800,
            "USD",
            new string[]{}
        ),

        new (
            "ad65b3e9-0b81-4f0e-9683-1f9f1a8fd14e",
            "ALC-0010",
            "Allocatr Analytics Dashboard",
            "Build an analytics dashboard for project performance, completion rates, and active user tracking.",
            "digital",
            "data visualization",
            DateTime.Parse("2025-09-25T08:00:00Z"),
            DateTime.Parse("2025-10-05T09:00:00Z"),
            DateOnly.Parse("2025-09-27"),
            DateOnly.Parse("2025-11-30"),
            "pending",
            15,
            "medium",
            "3c92ef72-beb2-4baf-b3db-0e8b8941a2de",
            new List<ProjectMemberDto>
            {
                new("6db0df26-f1b0-4dc0-8b7f-92ff084e02c9", "Data Analyst", true, DateTime.Parse("2025-02-02")),
            },
            4,
            10,
            DateTime.Parse("2025-10-07T15:00:00Z"),
            false,
            true,
            1200,
            "USD",
            new string[]{}
        ),
    };
}
