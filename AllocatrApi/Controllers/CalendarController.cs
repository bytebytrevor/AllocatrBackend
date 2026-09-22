

// using System.Globalization;
// using AllocatrApi.Dtos;
// using AllocatrApi.Models;
// using AllocatrApi.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;

// namespace AllocatrApi.Controllers;

// [ApiController]
// [Authorize]
// [Route("api/calendar")]
// public class CalendarController : ControllerBase
// {
//     private readonly UserManager<AllocatrUser> _userManager;
//     private readonly CalendarService _calendarService;
//     private readonly CalendarPlanningService _planningService;
//     private readonly ILogger<CalendarController> _logger;

//     public CalendarController(
//         UserManager<AllocatrUser> userManager,
//         CalendarService calendarService,
//         CalendarPlanningService planningService,
//         ILogger<CalendarController> logger)
//     {
//         _userManager = userManager;
//         _calendarService = calendarService;
//         _planningService = planningService;
//         _logger = logger;
//     }

//     /* =====================================================
//        CALENDAR EVENTS
//     ===================================================== */

//     [HttpGet]
//     public async Task<IActionResult> GetCalendar(
//         [FromQuery] string? start,
//         [FromQuery] string? end)
//     {
//         if (!TryParseRange(start, end, out var startDate, out var endDate, out var error))
//         {
//             return BadRequest(new { message = error });
//         }

//         try
//         {
//             var user = await _userManager.GetUserAsync(User);

//             if (user == null)
//             {
//                 return Unauthorized();
//             }

//             var events = await _calendarService.GetCalendarEventsAsync(
//                 user.Id,
//                 user.IsAllocat,
//                 startDate,
//                 endDate
//             );

//             return Ok(events);
//         }
//         catch (ArgumentException ex)
//         {
//             return BadRequest(new { message = ex.Message });
//         }
//         catch (InvalidOperationException ex)
//         {
//             _logger.LogError(ex, "Calendar database operation failed.");

//             return StatusCode(
//                 StatusCodes.Status503ServiceUnavailable,
//                 new
//                 {
//                     message = "The database is temporarily unavailable. Please try again."
//                 }
//             );
//         }
//     }

//     /* =====================================================
//        PERSONAL PLANNING BLOCKS
//     ===================================================== */

//     [HttpGet("plan-blocks")]
//     public async Task<IActionResult> GetPlanningBlocks(
//         [FromQuery] string? start,
//         [FromQuery] string? end)
//     {
//         if (!TryParseRange(start, end, out var startDate, out var endDate, out var error))
//         {
//             return BadRequest(new { message = error });
//         }

//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         var startAt = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
//         var endAt = endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

//         var blocks = await _planningService.GetPlanningBlocksAsync(
//             user.Id,
//             startAt,
//             endAt
//         );

//         return Ok(blocks);
//     }

//     [HttpPost("plan-blocks")]
//     public async Task<IActionResult> CreatePlanningBlock(
//         CreateCalendarPlanningBlockDto dto)
//     {
//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         try
//         {
//             var block = await _planningService.CreatePlanningBlockAsync(
//                 user.Id,
//                 user.IsAllocat,
//                 dto
//             );

//             return Ok(block);
//         }
//         catch (ArgumentException ex)
//         {
//             return BadRequest(new { message = ex.Message });
//         }
//     }

//     [HttpPatch("plan-blocks/{id:guid}")]
//     public async Task<IActionResult> UpdatePlanningBlock(
//         Guid id,
//         UpdateCalendarPlanningBlockDto dto)
//     {
//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         try
//         {
//             var block = await _planningService.UpdatePlanningBlockAsync(
//                 id,
//                 user.Id,
//                 user.IsAllocat,
//                 dto
//             );

//             if (block == null)
//             {
//                 return NotFound(new
//                 {
//                     message = "Planning block not found."
//                 });
//             }

//             return Ok(block);
//         }
//         catch (ArgumentException ex)
//         {
//             return BadRequest(new { message = ex.Message });
//         }
//     }

//     [HttpDelete("plan-blocks/{id:guid}")]
//     public async Task<IActionResult> DeletePlanningBlock(Guid id)
//     {
//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         var deleted = await _planningService.DeletePlanningBlockAsync(
//             id,
//             user.Id
//         );

//         if (!deleted)
//         {
//             return NotFound(new
//             {
//                 message = "Planning block not found."
//             });
//         }

//         return NoContent();
//     }

//     /* =====================================================
//        WEEKLY FOCUS
//     ===================================================== */

//     [HttpGet("focus")]
//     public async Task<IActionResult> GetFocusTasks(
//         [FromQuery] string? weekStart)
//     {
//         if (!TryParseDate(weekStart, out var week, out var error))
//         {
//             return BadRequest(new { message = error });
//         }

//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         var tasks = await _planningService.GetFocusTasksAsync(
//             user.Id,
//             week
//         );

//         return Ok(tasks);
//     }

//     [HttpPut("focus/{taskId:guid}")]
//     public async Task<IActionResult> AddFocusTask(
//         Guid taskId,
//         [FromQuery] string? weekStart)
//     {
//         if (!TryParseDate(weekStart, out var week, out var error))
//         {
//             return BadRequest(new { message = error });
//         }

//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         try
//         {
//             var tasks = await _planningService.AddFocusTaskAsync(
//                 user.Id,
//                 user.IsAllocat,
//                 taskId,
//                 week
//             );

//             return Ok(tasks);
//         }
//         catch (ArgumentException ex)
//         {
//             return BadRequest(new { message = ex.Message });
//         }
//     }

//     [HttpDelete("focus/{taskId:guid}")]
//     public async Task<IActionResult> RemoveFocusTask(
//         Guid taskId,
//         [FromQuery] string? weekStart)
//     {
//         if (!TryParseDate(weekStart, out var week, out var error))
//         {
//             return BadRequest(new { message = error });
//         }

//         var user = await _userManager.GetUserAsync(User);

//         if (user == null)
//         {
//             return Unauthorized();
//         }

//         var tasks = await _planningService.RemoveFocusTaskAsync(
//             user.Id,
//             taskId,
//             week
//         );

//         return Ok(tasks);
//     }

//     /* =====================================================
//        DATE PARSING
//     ===================================================== */

//     private static bool TryParseRange(
//         string? start,
//         string? end,
//         out DateOnly startDate,
//         out DateOnly endDate,
//         out string? error)
//     {
//         startDate = default;
//         endDate = default;
//         error = null;

//         if (!TryParseDate(start, out startDate, out _))
//         {
//             error = "Calendar start date must use yyyy-MM-dd format.";
//             return false;
//         }

//         if (!TryParseDate(end, out endDate, out _))
//         {
//             error = "Calendar end date must use yyyy-MM-dd format.";
//             return false;
//         }

//         if (endDate <= startDate)
//         {
//             error = "Calendar end date must be after the start date.";
//             return false;
//         }

//         return true;
//     }

//     private static bool TryParseDate(
//         string? value,
//         out DateOnly date,
//         out string? error)
//     {
//         date = default;
//         error = null;

//         if (string.IsNullOrWhiteSpace(value))
//         {
//             error = "A date is required.";
//             return false;
//         }

//         if (!DateOnly.TryParseExact(
//             value,
//             "yyyy-MM-dd",
//             CultureInfo.InvariantCulture,
//             DateTimeStyles.None,
//             out date))
//         {
//             error = "Date must use yyyy-MM-dd format.";
//             return false;
//         }

//         return true;
//     }
// }

using System.Globalization;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly CalendarService _calendarService;
    private readonly CalendarPlanningService _planningService;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(
        UserManager<AllocatrUser> userManager,
        CalendarService calendarService,
        CalendarPlanningService planningService,
        ILogger<CalendarController> logger)
    {
        _userManager = userManager;
        _calendarService = calendarService;
        _planningService = planningService;
        _logger = logger;
    }

    /* =====================================================
       CALENDAR EVENTS
    ===================================================== */

    [HttpGet]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] string? start,
        [FromQuery] string? end)
    {
        if (!TryParseRange(
            start,
            end,
            out var startDate,
            out var endDate,
            out var error))
        {
            return BadRequest(new { message = error });
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var events = await _calendarService.GetCalendarEventsAsync(
                user.Id,
                user.IsAllocat,
                startDate,
                endDate
            );

            return Ok(events);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Calendar database operation failed."
            );

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "The database is temporarily unavailable. Please try again."
                }
            );
        }
    }

    /* =====================================================
       PERSONAL PLANNING BLOCKS
    ===================================================== */

    [HttpGet("plan-blocks")]
    public async Task<IActionResult> GetPlanningBlocks(
        [FromQuery] string? start,
        [FromQuery] string? end)
    {
        if (!TryParseRange(
            start,
            end,
            out var startDate,
            out var endDate,
            out var error))
        {
            return BadRequest(new { message = error });
        }

        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var startAt = startDate.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc
        );

        var endAt = endDate.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc
        );

        var blocks = await _planningService.GetPlanningBlocksAsync(
            userId.Value,
            startAt,
            endAt
        );

        return Ok(blocks);
    }

    [HttpPost("plan-blocks")]
    public async Task<IActionResult> CreatePlanningBlock(
        CreateCalendarPlanningBlockDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var block = await _planningService.CreatePlanningBlockAsync(
                user.Id,
                user.IsAllocat,
                dto
            );

            return Ok(block);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("plan-blocks/{id:guid}")]
    public async Task<IActionResult> UpdatePlanningBlock(
        Guid id,
        UpdateCalendarPlanningBlockDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var block = await _planningService.UpdatePlanningBlockAsync(
                id,
                user.Id,
                user.IsAllocat,
                dto
            );

            if (block == null)
            {
                return NotFound(new
                {
                    message = "Planning block not found."
                });
            }

            return Ok(block);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("plan-blocks/{id:guid}")]
    public async Task<IActionResult> DeletePlanningBlock(Guid id)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var deleted = await _planningService.DeletePlanningBlockAsync(
            id,
            userId.Value
        );

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Planning block not found."
            });
        }

        return NoContent();
    }

    /* =====================================================
       WEEKLY FOCUS
    ===================================================== */

    [HttpGet("focus")]
    public async Task<IActionResult> GetFocusTasks(
        [FromQuery] string? weekStart)
    {
        if (!TryParseDate(
            weekStart,
            out var week,
            out var error))
        {
            return BadRequest(new { message = error });
        }

        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var tasks = await _planningService.GetFocusTasksAsync(
            userId.Value,
            week
        );

        return Ok(tasks);
    }

    [HttpPut("focus/{taskId:guid}")]
    public async Task<IActionResult> AddFocusTask(
        Guid taskId,
        [FromQuery] string? weekStart)
    {
        if (!TryParseDate(
            weekStart,
            out var week,
            out var error))
        {
            return BadRequest(new { message = error });
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            var tasks = await _planningService.AddFocusTaskAsync(
                user.Id,
                user.IsAllocat,
                taskId,
                week
            );

            return Ok(tasks);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("focus/{taskId:guid}")]
    public async Task<IActionResult> RemoveFocusTask(
        Guid taskId,
        [FromQuery] string? weekStart)
    {
        if (!TryParseDate(
            weekStart,
            out var week,
            out var error))
        {
            return BadRequest(new { message = error });
        }

        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var tasks = await _planningService.RemoveFocusTaskAsync(
            userId.Value,
            taskId,
            week
        );

        return Ok(tasks);
    }

    /* =====================================================
       CURRENT USER
    ===================================================== */

    private Guid? GetCurrentUserId()
    {
        var value = _userManager.GetUserId(User);

        return Guid.TryParse(value, out var userId)
            ? userId
            : null;
    }

    /* =====================================================
       DATE PARSING
    ===================================================== */

    private static bool TryParseRange(
        string? start,
        string? end,
        out DateOnly startDate,
        out DateOnly endDate,
        out string? error)
    {
        startDate = default;
        endDate = default;
        error = null;

        if (!TryParseDate(start, out startDate, out _))
        {
            error = "Calendar start date must use yyyy-MM-dd format.";
            return false;
        }

        if (!TryParseDate(end, out endDate, out _))
        {
            error = "Calendar end date must use yyyy-MM-dd format.";
            return false;
        }

        if (endDate <= startDate)
        {
            error = "Calendar end date must be after the start date.";
            return false;
        }

        return true;
    }

    private static bool TryParseDate(
        string? value,
        out DateOnly date,
        out string? error)
    {
        date = default;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "A date is required.";
            return false;
        }

        if (!DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date))
        {
            error = "Date must use yyyy-MM-dd format.";
            return false;
        }

        return true;
    }
}
