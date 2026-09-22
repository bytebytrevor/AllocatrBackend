using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class CalendarPlanningService
{
    private const int MaxFocusTasksPerWeek = 5;

    private readonly AllocatrDbContext _db;

    public CalendarPlanningService(AllocatrDbContext db)
    {
        _db = db;
    }

    /* =====================================================
       PLANNING BLOCKS
    ===================================================== */

    public async Task<List<CalendarPlanningBlockDto>> GetPlanningBlocksAsync(
        Guid userId,
        DateTime start,
        DateTime end)
    {
        start = EnsureUtc(start);
        end = EnsureUtc(end);

        return await _db.CalendarPlanningBlocks
            .AsNoTracking()
            .Where(block =>
                block.UserId == userId &&
                block.EndAt > start &&
                block.StartAt < end
            )
            .OrderBy(block => block.StartAt)
            .Select(block => new CalendarPlanningBlockDto(
                block.Id,
                block.ProjectId,
                block.TaskId,
                block.Project != null ? block.Project.Title : null,
                block.Title,
                block.Notes,
                block.StartAt,
                block.EndAt
            ))
            .ToListAsync();
    }

    public async Task<CalendarPlanningBlockDto> CreatePlanningBlockAsync(
        Guid userId,
        bool isAllocat,
        CreateCalendarPlanningBlockDto dto)
    {
        var title = dto.Title?.Trim() ?? string.Empty;
        var notes = dto.Notes?.Trim();
        var startAt = EnsureUtc(dto.StartAt);
        var endAt = EnsureUtc(dto.EndAt);

        ValidatePlanningBlock(title, notes, startAt, endAt);

        await ValidateReferencesAsync(
            userId,
            isAllocat,
            dto.ProjectId,
            dto.TaskId
        );

        var block = new CalendarPlanningBlock
        {
            UserId = userId,
            ProjectId = dto.ProjectId,
            TaskId = dto.TaskId,
            Title = title,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            StartAt = startAt,
            EndAt = endAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.CalendarPlanningBlocks.Add(block);
        await _db.SaveChangesAsync();

        return await GetPlanningBlockDtoAsync(block.Id, userId)
            ?? throw new InvalidOperationException(
                "The planning block was created but could not be reloaded."
            );
    }

    public async Task<CalendarPlanningBlockDto?> UpdatePlanningBlockAsync(
        Guid blockId,
        Guid userId,
        bool isAllocat,
        UpdateCalendarPlanningBlockDto dto)
    {
        var block = await _db.CalendarPlanningBlocks
            .FirstOrDefaultAsync(item =>
                item.Id == blockId &&
                item.UserId == userId
            );

        if (block == null)
        {
            return null;
        }

        var title = dto.Title?.Trim() ?? string.Empty;
        var notes = dto.Notes?.Trim();
        var startAt = EnsureUtc(dto.StartAt);
        var endAt = EnsureUtc(dto.EndAt);

        ValidatePlanningBlock(title, notes, startAt, endAt);

        await ValidateReferencesAsync(
            userId,
            isAllocat,
            dto.ProjectId,
            dto.TaskId
        );

        block.ProjectId = dto.ProjectId;
        block.TaskId = dto.TaskId;
        block.Title = title;
        block.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        block.StartAt = startAt;
        block.EndAt = endAt;
        block.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetPlanningBlockDtoAsync(block.Id, userId);
    }

    public async Task<bool> DeletePlanningBlockAsync(
        Guid blockId,
        Guid userId)
    {
        var block = await _db.CalendarPlanningBlocks
            .FirstOrDefaultAsync(item =>
                item.Id == blockId &&
                item.UserId == userId
            );

        if (block == null)
        {
            return false;
        }

        _db.CalendarPlanningBlocks.Remove(block);
        await _db.SaveChangesAsync();

        return true;
    }

    /* =====================================================
       WEEKLY FOCUS
    ===================================================== */

    public async Task<List<CalendarFocusTaskDto>> GetFocusTasksAsync(
        Guid userId,
        DateOnly weekStart)
    {
        return await _db.CalendarFocusTasks
            .AsNoTracking()
            .Where(focus =>
                focus.UserId == userId &&
                focus.WeekStart == weekStart
            )
            .OrderBy(focus => focus.CreatedAt)
            .Select(focus => new CalendarFocusTaskDto(
                focus.TaskId,
                focus.Task.ProjectId,
                focus.Task.Project.Title,
                focus.Task.Title,
                focus.Task.Status,
                focus.Task.DueDate
            ))
            .ToListAsync();
    }

    public async Task<List<CalendarFocusTaskDto>> AddFocusTaskAsync(
        Guid userId,
        bool isAllocat,
        Guid taskId,
        DateOnly weekStart)
    {
        var task = await _db.TaskItems
            .AsNoTracking()
            .Where(item => item.Id == taskId)
            .Select(item => new
            {
                item.Id,
                item.ProjectId
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            throw new ArgumentException("Task not found.");
        }

        if (!await CanAccessProjectAsync(userId, isAllocat, task.ProjectId))
        {
            throw new ArgumentException("Task is not available to your account.");
        }

        var alreadyFocused = await _db.CalendarFocusTasks.AnyAsync(focus =>
            focus.UserId == userId &&
            focus.TaskId == taskId &&
            focus.WeekStart == weekStart
        );

        if (alreadyFocused)
        {
            return await GetFocusTasksAsync(userId, weekStart);
        }

        var focusCount = await _db.CalendarFocusTasks.CountAsync(focus =>
            focus.UserId == userId &&
            focus.WeekStart == weekStart
        );

        if (focusCount >= MaxFocusTasksPerWeek)
        {
            throw new ArgumentException(
                $"You can focus on up to {MaxFocusTasksPerWeek} tasks per week."
            );
        }

        _db.CalendarFocusTasks.Add(new CalendarFocusTask
        {
            UserId = userId,
            TaskId = taskId,
            WeekStart = weekStart,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return await GetFocusTasksAsync(userId, weekStart);
    }

    public async Task<List<CalendarFocusTaskDto>> RemoveFocusTaskAsync(
        Guid userId,
        Guid taskId,
        DateOnly weekStart)
    {
        var focus = await _db.CalendarFocusTasks
            .FirstOrDefaultAsync(item =>
                item.UserId == userId &&
                item.TaskId == taskId &&
                item.WeekStart == weekStart
            );

        if (focus != null)
        {
            _db.CalendarFocusTasks.Remove(focus);
            await _db.SaveChangesAsync();
        }

        return await GetFocusTasksAsync(userId, weekStart);
    }

    /* =====================================================
       HELPERS
    ===================================================== */

    private async Task<CalendarPlanningBlockDto?> GetPlanningBlockDtoAsync(
        Guid blockId,
        Guid userId)
    {
        return await _db.CalendarPlanningBlocks
            .AsNoTracking()
            .Where(block =>
                block.Id == blockId &&
                block.UserId == userId
            )
            .Select(block => new CalendarPlanningBlockDto(
                block.Id,
                block.ProjectId,
                block.TaskId,
                block.Project != null ? block.Project.Title : null,
                block.Title,
                block.Notes,
                block.StartAt,
                block.EndAt
            ))
            .FirstOrDefaultAsync();
    }

    private async Task ValidateReferencesAsync(
        Guid userId,
        bool isAllocat,
        Guid? projectId,
        Guid? taskId)
    {
        if (projectId.HasValue &&
            !await CanAccessProjectAsync(userId, isAllocat, projectId.Value))
        {
            throw new ArgumentException(
                "The selected project is not available to your account."
            );
        }

        if (!taskId.HasValue)
        {
            return;
        }

        var task = await _db.TaskItems
            .AsNoTracking()
            .Where(item => item.Id == taskId.Value)
            .Select(item => new
            {
                item.ProjectId
            })
            .FirstOrDefaultAsync();

        if (task == null ||
            !await CanAccessProjectAsync(userId, isAllocat, task.ProjectId))
        {
            throw new ArgumentException(
                "The selected task is not available to your account."
            );
        }

        if (projectId.HasValue && task.ProjectId != projectId.Value)
        {
            throw new ArgumentException(
                "The selected task does not belong to the selected project."
            );
        }
    }

    private Task<bool> CanAccessProjectAsync(
        Guid userId,
        bool isAllocat,
        Guid projectId)
    {
        return _db.Projects
            .AsNoTracking()
            .AnyAsync(project =>
                project.Id == projectId &&
                (
                    project.UserId == userId ||
                    (
                        isAllocat &&
                        project.AllocatAssignments.Any(assignment =>
                            assignment.AllocatProfile.AllocatrUserId == userId &&
                            assignment.Status == ProjectAllocatStatus.Accepted &&
                            assignment.RemovedAt == null
                        )
                    )
                )
            );
    }

    private static void ValidatePlanningBlock(
        string title,
        string? notes,
        DateTime startAt,
        DateTime endAt)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Planning block title is required.");
        }

        if (title.Length > 180)
        {
            throw new ArgumentException(
                "Planning block title cannot exceed 180 characters."
            );
        }

        if (notes?.Length > 1200)
        {
            throw new ArgumentException(
                "Planning block notes cannot exceed 1200 characters."
            );
        }

        if (endAt <= startAt)
        {
            throw new ArgumentException(
                "Planning block end time must be after the start time."
            );
        }

        if (endAt - startAt > TimeSpan.FromHours(24))
        {
            throw new ArgumentException(
                "A planning block cannot be longer than 24 hours."
            );
        }
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
