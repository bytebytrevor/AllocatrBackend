using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class CalendarService
{
    private readonly AllocatrDbContext _db;

    public CalendarService(AllocatrDbContext db)
    {
        _db = db;
    }

    /* =====================================================
       GET CALENDAR EVENTS
    ===================================================== */

    public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(
        Guid userId,
        bool isAllocat,
        DateOnly start,
        DateOnly end)
    {
        if (end <= start)
        {
            throw new ArgumentException(
                "Calendar end date must be after the start date."
            );
        }

        /* =================================================
           ACCESSIBLE PROJECTS

           Normal user:
           - projects they own

           Allocat:
           - projects they own
           - accepted client projects
        ================================================= */

        var accessibleProjects = _db.Projects
            .AsNoTracking()
            .Where(p =>
                p.UserId == userId ||
                (
                    isAllocat &&
                    p.AllocatAssignments.Any(pa =>
                        pa.AllocatProfile.AllocatrUserId == userId &&
                        pa.Status == ProjectAllocatStatus.Accepted &&
                        pa.RemovedAt == null
                    )
                )
            );

        var projects = await accessibleProjects
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Title,
                p.ProjectCode,
                p.Status,
                p.StartDate,
                p.DueDate
            })
            .ToListAsync();

        var projectIds = projects
            .Select(project => project.Id)
            .ToList();

        var events = new List<CalendarEventDto>();

        /* =================================================
           PROJECT MILESTONES

           Project StartDate and DueDate are DateOnly values,
           so they remain all-day calendar events.
        ================================================= */

        foreach (var project in projects)
        {
            var relationship = project.UserId == userId
                ? "owner"
                : "allocat";

            if (
                project.StartDate is DateOnly projectStartDate &&
                projectStartDate >= start &&
                projectStartDate < end
            )
            {
                events.Add(new CalendarEventDto(
                    $"project-start-{project.Id}",
                    "project-start",
                    project.Id,
                    null,
                    project.Title,
                    project.ProjectCode,
                    $"{project.Title} starts",
                    null,
                    projectStartDate.ToDateTime(TimeOnly.MinValue),
                    null,
                    true,
                    project.Status,
                    relationship
                ));
            }

            if (
                project.DueDate is DateOnly projectDueDate &&
                projectDueDate >= start &&
                projectDueDate < end
            )
            {
                events.Add(new CalendarEventDto(
                    $"project-due-{project.Id}",
                    "project-due",
                    project.Id,
                    null,
                    project.Title,
                    project.ProjectCode,
                    $"{project.Title} due",
                    null,
                    projectDueDate.ToDateTime(TimeOnly.MinValue),
                    null,
                    true,
                    project.Status,
                    relationship
                ));
            }
        }

        /* =================================================
           TASK QUERY RANGE

           TaskItem.DueDate is stored by PostgreSQL as
           timestamp with time zone.

           Npgsql therefore requires UTC DateTime values.

           DateOnly.ToDateTime() normally creates Kind =
           Unspecified, so explicitly create UTC boundaries.
        ================================================= */

        var startDateTime = start.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc
        );

        var endDateTime = end.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc
        );

        /* =================================================
           TASK EVENTS
        ================================================= */

        var tasks = await _db.TaskItems
            .AsNoTracking()
            .Where(task =>
                projectIds.Contains(task.ProjectId) &&
                task.DueDate.HasValue &&
                task.DueDate.Value >= startDateTime &&
                task.DueDate.Value < endDateTime
            )
            .Select(task => new
            {
                task.Id,
                task.ProjectId,
                task.Title,
                task.Description,
                task.Status,
                task.DueDate
            })
            .ToListAsync();

        var projectMap = projects.ToDictionary(
            project => project.Id
        );

        foreach (var task in tasks)
        {
            if (!task.DueDate.HasValue)
            {
                continue;
            }

            if (!projectMap.TryGetValue(task.ProjectId, out var project))
            {
                continue;
            }

            var dueDate = task.DueDate.Value;

            /*
             * DueDate comes from PostgreSQL timestamptz and should
             * already be UTC. This is only a defensive normalization.
             */
            if (dueDate.Kind != DateTimeKind.Utc)
            {
                dueDate = DateTime.SpecifyKind(
                    dueDate,
                    DateTimeKind.Utc
                );
            }

            var relationship = project.UserId == userId
                ? "owner"
                : "allocat";

            /*
             * A task at exact midnight can be represented as an
             * all-day item. Timed tasks remain in the week timeline.
             */
            var allDay =
                dueDate.Hour == 0 &&
                dueDate.Minute == 0 &&
                dueDate.Second == 0;

            events.Add(new CalendarEventDto(
                $"task-{task.Id}",
                "task",
                project.Id,
                task.Id,
                project.Title,
                project.ProjectCode,
                task.Title,
                task.Description,
                dueDate,
                null,
                allDay,
                task.Status,
                relationship
            ));
        }

        /* =================================================
           SORT EVENTS
        ================================================= */

        return events
            .OrderBy(calendarEvent => calendarEvent.Start)
            .ThenBy(calendarEvent => calendarEvent.Title)
            .ToList();
    }
}