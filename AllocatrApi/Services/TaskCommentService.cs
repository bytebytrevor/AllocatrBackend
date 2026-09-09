// using AllocatrApi.Data;
// using AllocatrApi.Dtos;
// using AllocatrApi.Models;
// using Microsoft.EntityFrameworkCore;

// namespace AllocatrApi.Services;

// public class TaskCommentService
// {
//     private readonly AllocatrDbContext _db;

//     public TaskCommentService(AllocatrDbContext db)
//     {
//         _db = db;
//     }

//     /* --------------------------------------------------------
//      * READ
//      * -------------------------------------------------------- */

//     public async Task<List<TaskCommentDto>> GetTaskCommentsAsync(
//         Guid taskId
//     )
//     {
//         return await _db.TaskComments
//             .Where(c => c.TaskItemId == taskId)
//             .OrderBy(c => c.CreatedAt)
//             .Select(c => new TaskCommentDto(
//                 c.Id,
//                 c.CreatedAt,
//                 c.UpdatedAt,
//                 c.Comment,
//                 c.CreatedById,
//                 c.CreatedBy.FullName ?? "Allocatr user",
//                 c.CreatedBy.AvatarUrl,
//                 c.TaskItemId
//             ))
//             .ToListAsync();
//     }

//     /* --------------------------------------------------------
//      * CREATE
//      * -------------------------------------------------------- */

//     public async Task<TaskCommentDto?> CreateCommentAsync(
//         Guid taskId,
//         Guid userId,
//         CreateTaskCommentDto request
//     )
//     {
//         var taskExists = await _db.TaskItems
//             .AnyAsync(t => t.Id == taskId);

//         if (!taskExists)
//         {
//             return null;
//         }

//         var comment = request.Comment.Trim();

//         if (string.IsNullOrWhiteSpace(comment))
//         {
//             throw new ArgumentException(
//                 "Comment cannot be empty."
//             );
//         }

//         if (comment.Length > 2000)
//         {
//             throw new ArgumentException(
//                 "Comment cannot exceed 2000 characters."
//             );
//         }

//         var taskComment = new TaskComment
//         {
//             Id = Guid.NewGuid(),
//             TaskItemId = taskId,
//             CreatedById = userId,
//             Comment = comment,
//             CreatedAt = DateTime.UtcNow,
//         };

//         _db.TaskComments.Add(taskComment);

//         await _db.SaveChangesAsync();

//         return await _db.TaskComments
//             .Where(c => c.Id == taskComment.Id)
//             .Select(c => new TaskCommentDto(
//                 c.Id,
//                 c.CreatedAt,
//                 c.UpdatedAt,
//                 c.Comment,
//                 c.CreatedById,
//                 c.CreatedBy.FullName ?? "Allocatr user",
//                 c.CreatedBy.AvatarUrl,
//                 c.TaskItemId
//             ))
//             .FirstAsync();
//     }

//     /* --------------------------------------------------------
//      * UPDATE
//      * -------------------------------------------------------- */

//     public async Task<TaskCommentDto?> UpdateCommentAsync(
//         Guid taskId,
//         Guid commentId,
//         Guid userId,
//         string comment
//     )
//     {
//         var existing = await _db.TaskComments
//             .FirstOrDefaultAsync(c =>
//                 c.Id == commentId &&
//                 c.TaskItemId == taskId &&
//                 c.CreatedById == userId
//             );

//         if (existing == null)
//         {
//             return null;
//         }

//         var cleanedComment = comment.Trim();

//         if (string.IsNullOrWhiteSpace(cleanedComment))
//         {
//             throw new ArgumentException(
//                 "Comment cannot be empty."
//             );
//         }

//         if (cleanedComment.Length > 2000)
//         {
//             throw new ArgumentException(
//                 "Comment cannot exceed 2000 characters."
//             );
//         }

//         existing.Comment = cleanedComment;
//         existing.UpdatedAt = DateTime.UtcNow;

//         await _db.SaveChangesAsync();

//         return await _db.TaskComments
//             .Where(c => c.Id == existing.Id)
//             .Select(c => new TaskCommentDto(
//                 c.Id,
//                 c.CreatedAt,
//                 c.UpdatedAt,
//                 c.Comment,
//                 c.CreatedById,
//                 c.CreatedBy.FullName ?? "Allocatr user",
//                 c.CreatedBy.AvatarUrl,
//                 c.TaskItemId
//             ))
//             .FirstAsync();
//     }

//     /* --------------------------------------------------------
//      * DELETE
//      * -------------------------------------------------------- */

//     public async Task<bool> DeleteCommentAsync(
//         Guid taskId,
//         Guid commentId,
//         Guid userId
//     )
//     {
//         var comment = await _db.TaskComments
//             .FirstOrDefaultAsync(c =>
//                 c.Id == commentId &&
//                 c.TaskItemId == taskId &&
//                 c.CreatedById == userId
//             );

//         if (comment == null)
//         {
//             return false;
//         }

//         _db.TaskComments.Remove(comment);

//         await _db.SaveChangesAsync();

//         return true;
//     }
// }

using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class TaskCommentService
{
    private readonly AllocatrDbContext _db;
    private readonly ProjectAccessService _projectAccess;

    public TaskCommentService(
        AllocatrDbContext db,
        ProjectAccessService projectAccess)
    {
        _db = db;
        _projectAccess = projectAccess;
    }

    public async Task<List<TaskCommentDto>?> GetTaskCommentsAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return null;
        }

        var canView = await _projectAccess.CanViewProjectAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return null;
        }

        return await _db.TaskComments
            .AsNoTracking()
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.CreatedAt,
                c.UpdatedAt,
                c.Comment,
                c.CreatedById,
                c.CreatedBy.FullName ?? "Allocatr user",
                c.CreatedBy.AvatarUrl,
                c.TaskItemId
            ))
            .ToListAsync();
    }

    public async Task<TaskCommentDto?> CreateCommentAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat,
        CreateTaskCommentDto request)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return null;
        }

        var canView = await _projectAccess.CanViewProjectAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return null;
        }

        var comment =
            request.Comment?.Trim() ?? string.Empty;

        ValidateComment(comment);

        var taskComment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            CreatedById = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.TaskComments.Add(taskComment);
        await _db.SaveChangesAsync();

        return await GetCommentByIdAsync(taskComment.Id);
    }

    public async Task<TaskCommentDto?> UpdateCommentAsync(
        Guid taskId,
        Guid commentId,
        Guid userId,
        bool isAllocat,
        string comment)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return null;
        }

        var canView = await _projectAccess.CanViewProjectAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return null;
        }

        var existing = await _db.TaskComments
            .FirstOrDefaultAsync(c =>
                c.Id == commentId &&
                c.TaskItemId == taskId &&
                c.CreatedById == userId
            );

        if (existing == null)
        {
            return null;
        }

        var cleanedComment =
            comment?.Trim() ?? string.Empty;

        ValidateComment(cleanedComment);

        existing.Comment = cleanedComment;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetCommentByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteCommentAsync(
        Guid taskId,
        Guid commentId,
        Guid userId,
        bool isAllocat)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return false;
        }

        var canView = await _projectAccess.CanViewProjectAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return false;
        }

        var comment = await _db.TaskComments
            .FirstOrDefaultAsync(c =>
                c.Id == commentId &&
                c.TaskItemId == taskId &&
                c.CreatedById == userId
            );

        if (comment == null)
        {
            return false;
        }

        _db.TaskComments.Remove(comment);
        await _db.SaveChangesAsync();

        return true;
    }

    private async Task<Guid?> GetTaskProjectIdAsync(Guid taskId)
    {
        return await _db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => (Guid?)t.ProjectId)
            .FirstOrDefaultAsync();
    }

    private async Task<TaskCommentDto?> GetCommentByIdAsync(
        Guid commentId)
    {
        return await _db.TaskComments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.CreatedAt,
                c.UpdatedAt,
                c.Comment,
                c.CreatedById,
                c.CreatedBy.FullName ?? "Allocatr user",
                c.CreatedBy.AvatarUrl,
                c.TaskItemId
            ))
            .FirstOrDefaultAsync();
    }

    private static void ValidateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException(
                "Comment cannot be empty."
            );
        }

        if (comment.Length > 2000)
        {
            throw new ArgumentException(
                "Comment cannot exceed 2000 characters."
            );
        }
    }
}